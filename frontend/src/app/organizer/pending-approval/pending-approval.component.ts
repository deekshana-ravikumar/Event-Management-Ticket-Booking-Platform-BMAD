import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-pending-approval',
  standalone: true,
  imports: [RouterModule, MatButtonModule, MatIconModule, MatCardModule],
  template: `
    <div class="auth-page">
      <mat-card class="auth-card">
        <mat-card-content class="centered-content">
          <mat-icon class="large-icon pending-icon">hourglass_top</mat-icon>
          <h2>Pending Admin Approval</h2>
          <p>
            Your organisation is pending admin approval.<br />
            You'll receive an email once your organisation has been reviewed.
          </p>
          <p class="hint">In the meantime, you can browse public events as an attendee.</p>
          <button mat-stroked-button color="warn" (click)="logout()">Logout</button>
        </mat-card-content>
      </mat-card>
    </div>
  `,
})
export class PendingApprovalComponent {
  constructor(private auth: AuthService, private router: Router) {}

  logout(): void {
    this.auth.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login']),
    });
  }
}
