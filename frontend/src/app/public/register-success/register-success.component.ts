import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-register-success',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule, MatCardModule],
  template: `
    <div class="auth-page">
      <mat-card class="auth-card">
        <mat-card-content class="centered-content">
          <mat-icon color="primary" class="large-icon">mark_email_read</mat-icon>
          <h2>Check Your Email!</h2>
          <p>
            We sent a verification link to <strong>{{ email }}</strong>.<br />
            It expires in <strong>24 hours</strong>.
          </p>
          <p class="hint">Didn't receive it? Check your spam folder or <a routerLink="/verify-email">request a new link</a>.</p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
})
export class RegisterSuccessComponent implements OnInit {
  email = '';

  constructor(private router: Router) {}

  ngOnInit(): void {
    const state = this.router.getCurrentNavigation()?.extras.state as { email?: string } | undefined;
    this.email = state?.email ?? window.history.state?.email ?? '';
  }
}
