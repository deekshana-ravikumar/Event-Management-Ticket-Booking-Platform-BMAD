import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule,
  ],
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  serverError = '';
  unverifiedEmail = '';
  hidePassword = true;
  resendLoading = false;
  resendMessage = '';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.serverError = '';
    this.unverifiedEmail = '';
    const { email, password } = this.form.value;

    this.auth.login(email, password).subscribe({
      next: (res) => {
        this.loading = false;
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        if (returnUrl) {
          this.router.navigateByUrl(decodeURIComponent(returnUrl));
        } else {
          this.router.navigateByUrl(this.auth.defaultRouteForRole());
        }
      },
      error: (err) => {
        this.loading = false;
        const detail: string = err.error?.detail ?? '';
        if (detail.toLowerCase().includes('verify your email')) {
          this.unverifiedEmail = email;
        } else {
          this.serverError = detail || 'Invalid email or password.';
        }
      },
    });
  }

  resendVerification(): void {
    this.resendLoading = true;
    this.resendMessage = '';
    this.http
      .post('/api/auth/resend-verification', { email: this.unverifiedEmail })
      .subscribe({
        next: () => {
          this.resendLoading = false;
          this.resendMessage = 'Verification email resent! Check your inbox.';
        },
        error: () => {
          this.resendLoading = false;
          this.resendMessage = 'Failed to resend. Please try again later.';
        },
      });
  }
}
