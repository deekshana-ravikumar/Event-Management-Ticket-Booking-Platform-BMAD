import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password');
  const confirm = control.get('confirmPassword');
  if (password && confirm && password.value !== confirm.value) {
    return { passwordMismatch: true };
  }
  return null;
}

function passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
  const value: string = control.value ?? '';
  const errors: Record<string, boolean> = {};
  if (value.length < 8) errors['minLength'] = true;
  if (!/[A-Z]/.test(value)) errors['uppercase'] = true;
  if (!/[a-z]/.test(value)) errors['lowercase'] = true;
  if (!/[0-9]/.test(value)) errors['digit'] = true;
  if (!/[^A-Za-z0-9]/.test(value)) errors['special'] = true;
  return Object.keys(errors).length ? errors : null;
}

@Component({
  selector: 'app-register-organizer',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatCardModule,
  ],
  templateUrl: './register-organizer.component.html',
})
export class RegisterOrganizerComponent implements OnInit {
  step1!: FormGroup;
  step2!: FormGroup;
  step3!: FormGroup;
  step4!: FormGroup;

  loading = false;
  serverError = '';
  hidePassword = true;
  hideConfirm = true;

  readonly categories = [
    'Music', 'Technology', 'Business', 'Sports', 'Arts & Culture',
    'Food & Drink', 'Charity & Causes', 'Education', 'Fashion', 'Health & Wellness', 'Other',
  ];

  get passwordStrengthLevel(): number {
    const pw: string = this.step1?.get('password')?.value ?? '';
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw)) score++;
    if (/[a-z]/.test(pw)) score++;
    if (/[0-9]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return score;
  }

  get passwordStrengthLabel(): string {
    return ['', 'Very Weak', 'Weak', 'Fair', 'Strong', 'Very Strong'][this.passwordStrengthLevel] ?? '';
  }

  get passwordStrengthColor(): string {
    return ['warn', 'warn', 'warn', 'accent', 'primary', 'primary'][this.passwordStrengthLevel] ?? 'warn';
  }

  get passwordErrors() {
    return this.step1?.get('password')?.errors ?? {};
  }

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    this.step1 = this.fb.group(
      {
        orgName: ['', [Validators.required, Validators.maxLength(200)]],
        contactPerson: ['', [Validators.required, Validators.maxLength(100)]],
        email: ['', [Validators.required, Validators.email]],
        phone: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
        category: ['', Validators.required],
        password: ['', [Validators.required, passwordStrengthValidator]],
        confirmPassword: ['', Validators.required],
      },
      { validators: passwordMatchValidator }
    );

    this.step2 = this.fb.group({
      address: ['', [Validators.required, Validators.maxLength(300)]],
      city: ['', [Validators.required, Validators.maxLength(100)]],
      state: ['', [Validators.required, Validators.maxLength(100)]],
      pincode: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
      website: ['', Validators.maxLength(200)],
      socialLinks: ['', Validators.maxLength(500)],
    });

    this.step3 = this.fb.group({
      panNumber: [''],
      gstin: [''],
      idProof: [''],
      businessRegistration: [''],
    });

    this.step4 = this.fb.group({
      termsAccepted: [false, Validators.requiredTrue],
    });
  }

  onSubmit(): void {
    if (!this.step4.valid) {
      this.step4.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.serverError = '';

    const payload = {
      ...this.step1.value,
      ...this.step2.value,
    };

    this.http.post('/api/auth/register/organizer', payload).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/register/success'], {
          state: { email: this.step1.value.email },
        });
      },
      error: (err) => {
        this.loading = false;
        this.serverError =
          err.error?.detail ??
          err.error?.title ??
          'Registration failed. Please try again.';
      },
    });
  }
}
