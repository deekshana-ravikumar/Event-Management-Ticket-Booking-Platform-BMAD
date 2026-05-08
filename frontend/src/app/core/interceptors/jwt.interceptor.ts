import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;

export const jwtInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getAccessToken();
  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !isRefreshing && !req.url.includes('/api/auth/refresh')) {
        isRefreshing = true;
        return auth.refreshToken().pipe(
          switchMap(() => {
            isRefreshing = false;
            const retryToken = auth.getAccessToken();
            const retryReq = retryToken
              ? req.clone({ setHeaders: { Authorization: `Bearer ${retryToken}` } })
              : req;
            return next(retryReq);
          }),
          catchError((refreshErr) => {
            isRefreshing = false;
            const returnUrl = encodeURIComponent(router.url);
            router.navigate(['/login'], { queryParams: { returnUrl } });
            return throwError(() => refreshErr);
          })
        );
      }
      return throwError(() => err);
    })
  );
};
