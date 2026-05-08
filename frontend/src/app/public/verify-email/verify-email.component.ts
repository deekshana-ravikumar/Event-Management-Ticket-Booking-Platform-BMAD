import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

type VerifyState = 'loading' | 'success' | 'error' | 'resent' | 'resend-form';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatCardModule,
  ],
  templateUrl: './verify-email.component.html',
})
export class VerifyEmailComponent implements OnInit {
  state: VerifyState = 'loading';
  errorMessage = '';
  resendLoading = false;
  resendError = '';
  resendForm!: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.resendForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });

    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.state = 'resend-form';
      return;
    }
    // Backend: POST /api/auth/verify-email { token }
    this.http.post('/api/auth/verify-email', { token }).subscribe({
      next: () => { this.state = 'success'; },
      error: (err: HttpErrorResponse) => {
        this.state = 'error';
        this.errorMessage = err.error?.detail ?? 'Verification failed.';
      },
    });
  }

  requestNewLink(): void {
    this.state = 'resend-form';
  }

  submitResend(): void {
    if (this.resendForm.invalid) {
      this.resendForm.markAllAsTouched();
      return;
    }
    this.resendLoading = true;
    this.resendError = '';
    this.http
      .post('/api/auth/resend-verification', { email: this.resendForm.value.email })
      .subscribe({
        next: () => {
          this.resendLoading = false;
          this.state = 'resent';
        },
        error: (err: HttpErrorResponse) => {
          this.resendLoading = false;
          if (err.status === 429) {
            this.resendError = 'Please wait before requesting another link.';
          } else {
            this.resendError = 'Failed to send verification email. Please try again.';
          }
        },
      });
  }
}
