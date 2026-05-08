import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => http.verify());

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  describe('login()', () => {
    it('stores access token in memory (not localStorage)', () => {
      const mockResponse = {
        accessToken: 'test-token',
        expiresIn: 1800,
        userId: 'u1',
        email: 'raj@example.com',
        role: 'Attendee' as const,
      };

      service.login('raj@example.com', 'P@ssw0rd!').subscribe();

      const req = http.expectOne('/api/auth/login');
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);

      expect(service.getAccessToken()).toBe('test-token');
      expect(localStorage.getItem('accessToken')).toBeNull();
      expect(service.currentUser$.value?.email).toBe('raj@example.com');
      expect(service.currentUser$.value?.role).toBe('Attendee');
    });

    it('stores orgStatus for Organizer role', () => {
      const mockResponse = {
        accessToken: 'org-token',
        expiresIn: 1800,
        userId: 'o1',
        email: 'org@example.com',
        role: 'Organizer' as const,
        orgStatus: 'PendingApproval' as const,
      };

      service.login('org@example.com', 'P@ssw0rd!').subscribe();

      const req = http.expectOne('/api/auth/login');
      req.flush(mockResponse);

      expect(service.currentUser$.value?.orgStatus).toBe('PendingApproval');
    });
  });

  describe('logout()', () => {
    it('clears token and calls POST /logout', () => {
      // Seed a token first
      service.login('raj@example.com', 'P@ssw0rd!').subscribe();
      http.expectOne('/api/auth/login').flush({
        accessToken: 'tok', expiresIn: 1800, userId: 'u1', email: 'raj@example.com', role: 'Attendee',
      });

      service.logout().subscribe();
      const req = http.expectOne('/api/auth/logout');
      expect(req.request.method).toBe('POST');
      req.flush(null, { status: 204, statusText: 'No Content' });

      expect(service.getAccessToken()).toBeNull();
      expect(service.currentUser$.value).toBeNull();
    });

    it('clears session even if server returns error', () => {
      service.login('raj@example.com', 'P@ssw0rd!').subscribe();
      http.expectOne('/api/auth/login').flush({
        accessToken: 'tok', expiresIn: 1800, userId: 'u1', email: 'raj@example.com', role: 'Attendee',
      });

      service.logout().subscribe({ error: () => {} });
      http.expectOne('/api/auth/logout').error(new ErrorEvent('network'));

      expect(service.getAccessToken()).toBeNull();
    });
  });

  describe('isAuthenticated()', () => {
    it('returns false when no token present', () => {
      expect(service.isAuthenticated()).toBeFalse();
    });

    it('returns true after successful login', () => {
      service.login('r@e.com', 'P@ss1!').subscribe();
      http.expectOne('/api/auth/login').flush({
        accessToken: 'tok', expiresIn: 1800, userId: 'u1', email: 'r@e.com', role: 'Attendee',
      });
      expect(service.isAuthenticated()).toBeTrue();
    });
  });
});
