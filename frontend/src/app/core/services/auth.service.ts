import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { User, LoginResponse, UserRole } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = '/api/auth';

  /** Access token stored in memory only — never in localStorage */
  private accessToken: string | null = null;

  readonly currentUser$ = new BehaviorSubject<User | null>(null);

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API}/login`, { email, password }, { withCredentials: true }).pipe(
      tap((res) => {
        this.accessToken = res.accessToken;
        this.currentUser$.next({
          userId: res.userId,
          email: res.email,
          role: res.role,
          orgStatus: res.orgStatus,
        });
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.API}/logout`, {}, { withCredentials: true }).pipe(
      tap(() => this.clearSession()),
      catchError((err) => {
        this.clearSession();
        return throwError(() => err);
      })
    );
  }

  refreshToken(): Observable<{ accessToken: string; expiresIn: number }> {
    return this.http
      .post<{ accessToken: string; expiresIn: number }>(`${this.API}/refresh`, {}, { withCredentials: true })
      .pipe(
        tap((res) => {
          this.accessToken = res.accessToken;
        })
      );
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  isAuthenticated(): boolean {
    return this.accessToken !== null;
  }

  hasRole(role: UserRole): boolean {
    return this.currentUser$.value?.role === role;
  }

  private clearSession(): void {
    this.accessToken = null;
    this.currentUser$.next(null);
  }

  defaultRouteForRole(): string {
    const role = this.currentUser$.value?.role;
    switch (role) {
      case 'Organizer': return '/organizer/dashboard';
      case 'SuperAdmin': return '/admin/dashboard';
      case 'CheckinStaff': return '/checkin/events';
      default: return '/my/dashboard';
    }
  }
}
