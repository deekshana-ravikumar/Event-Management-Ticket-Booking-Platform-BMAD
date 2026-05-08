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
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
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
  selector: 'app-register-attendee',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
  ],
  templateUrl: './register-attendee.component.html',
})
export class RegisterAttendeeComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  serverError = '';
  hidePassword = true;
  hideConfirm = true;

  get passwordStrengthLevel(): number {
    const pw: string = this.form?.get('password')?.value ?? '';
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw)) score++;
    if (/[a-z]/.test(pw)) score++;
    if (/[0-9]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return score;
  }

  get passwordStrengthLabel(): string {
    const labels = ['', 'Very Weak', 'Weak', 'Fair', 'Strong', 'Very Strong'];
    return labels[this.passwordStrengthLevel] ?? '';
  }

  get passwordStrengthColor(): string {
    const colors = ['warn', 'warn', 'warn', 'accent', 'primary', 'primary'];
    return colors[this.passwordStrengthLevel] ?? 'warn';
  }

  get passwordErrors() {
    return this.form?.get('password')?.errors ?? {};
  }

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    this.form = this.fb.group(
      {
        fullName: ['', [Validators.required, Validators.maxLength(100)]],
        email: ['', [Validators.required, Validators.email]],
        phone: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
        city: ['', [Validators.required, Validators.maxLength(100)]],
        password: ['', [Validators.required, passwordStrengthValidator]],
        confirmPassword: ['', Validators.required],
        termsAccepted: [false, Validators.requiredTrue],
      },
      { validators: passwordMatchValidator }
    );
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.serverError = '';
    const payload = this.form.value;
    this.http.post('/api/auth/register/attendee', { ...payload, tncVersion: '1.0' }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/register/success'], {
          state: { email: this.form.value.email },
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
