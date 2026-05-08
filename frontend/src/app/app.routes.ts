import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { organizerGuard } from './core/guards/organizer.guard';

export const routes: Routes = [
  // Public
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'register/attendee',
    loadComponent: () =>
      import('./public/register-attendee/register-attendee.component').then(
        (m) => m.RegisterAttendeeComponent
      ),
  },
  {
    path: 'register/organizer',
    loadComponent: () =>
      import('./public/register-organizer/register-organizer.component').then(
        (m) => m.RegisterOrganizerComponent
      ),
  },
  {
    path: 'register/success',
    loadComponent: () =>
      import('./public/register-success/register-success.component').then(
        (m) => m.RegisterSuccessComponent
      ),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./public/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'verify-email',
    loadComponent: () =>
      import('./public/verify-email/verify-email.component').then(
        (m) => m.VerifyEmailComponent
      ),
  },
  // Organizer pending-approval (accessible without active org status)
  {
    path: 'organizer/pending-approval',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./organizer/pending-approval/pending-approval.component').then(
        (m) => m.PendingApprovalComponent
      ),
  },
  // Organizer portal (requires active org)
  {
    path: 'organizer/dashboard',
    canActivate: [authGuard, organizerGuard],
    loadComponent: () =>
      import('./organizer/dashboard/organizer-dashboard.component').then(
        (m) => m.OrganizerDashboardComponent
      ),
  },
  // Attendee portal
  {
    path: 'my/dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./attendee/dashboard/attendee-dashboard.component').then(
        (m) => m.AttendeeDashboardComponent
      ),
  },
  { path: '**', redirectTo: 'login' },
];
