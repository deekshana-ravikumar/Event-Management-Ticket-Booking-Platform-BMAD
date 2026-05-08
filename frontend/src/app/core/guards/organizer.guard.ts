import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const organizerGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  const user = auth.currentUser$.value;
  if (user?.orgStatus !== 'Active') {
    router.navigate(['/organizer/pending-approval']);
    return false;
  }

  return true;
};
