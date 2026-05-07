# Smart Event Management & Ticket Booking Platform — Architecture Reference

**Document Owner:** Winston (System Architect)  
**Last Updated:** 2026-05-07  
**Status:** Authoritative — aligned to DEV-01A implemented backend  
**Source of Truth:** DEV-00, DEV-01 (both phases), BA/PM/UX artifacts, implemented codebase  
**Audience:** All future DEV story implementors, onboarding engineers, QA, DevOps  

> This document supersedes all pre-implementation architecture sketches. It reflects what was _built and reviewed_, not what was initially planned. Deviations from planning artifacts are explicitly noted.

---

## Table of Contents

1. [Overall Solution Architecture](#1-overall-solution-architecture)
2. [Clean Architecture Layering Strategy](#2-clean-architecture-layering-strategy)
3. [Backend Project Structure](#3-backend-project-structure)
4. [Angular Frontend Architecture](#4-angular-frontend-architecture)
5. [Database Architecture & Entity Relationships](#5-database-architecture--entity-relationships)
6. [Authentication & Authorization Architecture](#6-authentication--authorization-architecture)
7. [JWT + Refresh Token Flow](#7-jwt--refresh-token-flow)
8. [Organizer Approval & Access Architecture](#8-organizer-approval--access-architecture)
9. [QR Generation & Validation Architecture](#9-qr-generation--validation-architecture)
10. [Booking & Inventory Consistency Strategy](#10-booking--inventory-consistency-strategy)
11. [Scheduler / Background Job Architecture](#11-scheduler--background-job-architecture)
12. [API Module Architecture](#12-api-module-architecture)
13. [Cross-Cutting Concerns](#13-cross-cutting-concerns)
14. [Environment & Configuration Architecture](#14-environment--configuration-architecture)
15. [Deployment Architecture](#15-deployment-architecture)
16. [Security Architecture](#16-security-architecture)
17. [Architecture Decision Records (ADRs)](#17-architecture-decision-records-adrs)
18. [Technical Debt Register](#18-technical-debt-register)
19. [Future Scalability Guidance](#19-future-scalability-guidance)
20. [Sprint 5 Booking-Locking Architectural Preparation](#20-sprint-5-booking-locking-architectural-preparation)

---

## 1. Overall Solution Architecture

### 1.1 Architectural Style

**Modular Monolith** for V1. A single deployable ASP.NET Core Web API and a single Angular SPA. Module boundaries inside the monolith are kept clean enough to extract individual services (Notification, Reporting) in Phase 2 without restructuring the rest of the system.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              BROWSER                                         │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  Angular SPA (Angular 17+ LTS, standalone components)               │   │
│  │  PublicModule | AttendeeModule | OrganizerModule | AdminModule       │   │
│  │  CheckinModule — all lazy-loaded behind route guards                 │   │
│  └──────────────────────────┬───────────────────────────────────────────┘   │
└─────────────────────────────┼───────────────────────────────────────────────┘
                              │  HTTPS + JWT (Authorization: Bearer)
                              │  httpOnly Cookie (refresh token)
                              ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     nginx (TLS termination + reverse proxy)                  │
│  /api/** → ASP.NET Core container                                            │
│  /       → Angular static files                                              │
└──────────┬──────────────────────────────────────────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  ASP.NET Core Web API  (EventManagement.API)                                  │
│  ─────────────────────────────────────────────────────────────────────────  │
│  Controllers (thin)          ← HTTP surface only; no business logic           │
│  Middleware pipeline          CorrelationId → Auth → Authz → Controllers      │
│  Authorization policies       ActiveOrganizer, AdminOnly, CheckinStaff        │
│  GlobalExceptionHandler       ProblemDetails for all error paths               │
│  CustomAuthorizationResultHandler  Consistent 401/403 ProblemDetails          │
│  ─────────────────────────────────────────────────────────────────────────  │
│  Application Layer  (EventManagement.Application)                             │
│  MediatR CQRS    Commands / Queries / Handlers / Validators                  │
│  FluentValidation pipeline behavior (ValidationBehavior<TRequest,TResponse>) │
│  Interfaces (IAppDbContext, IEmailService, IJwtTokenService, etc.)            │
│  ─────────────────────────────────────────────────────────────────────────  │
│  Domain Layer  (EventManagement.Domain)                                       │
│  Entities, Enums, Domain Exceptions (zero framework dependencies)             │
│  ─────────────────────────────────────────────────────────────────────────  │
│  Infrastructure Layer  (EventManagement.Infrastructure)                       │
│  AppDbContext (EF Core 8, Pomelo MySQL)                                       │
│  JwtTokenService, PasswordHasher, EmailService, ConsentLedgerService         │
│  Future: HangfireScheduler, QRCodeService, StorageProvider, AuditWriter      │
└──────┬────────────────────────────────────────────────────────────────────────┘
       │                                │                         │
       ▼                                ▼                         ▼
┌──────────────┐             ┌────────────────────┐   ┌─────────────────────┐
│  MySQL 8.x   │             │  SMTP Provider     │   │  Local Filesystem   │
│  (Docker)    │             │  (MailHog dev /     │   │  (IStorageProvider) │
│              │             │   Brevo prod)       │   │                     │
└──────────────┘             └────────────────────┘   └─────────────────────┘
```

### 1.2 Core Design Principles

| Principle | Application |
|-----------|------------|
| **Boring technology** | MySQL 8, ASP.NET Core, Angular, EF Core — all battle-tested, hire-able skills |
| **Developer productivity** | MediatR eliminates controller bloat; FluentValidation keeps validators discoverable; DI via extension methods |
| **Stateless API** | No server-side session; JWT carries identity; horizontal scaling possible with no refactor |
| **Interface-first for I/O** | `IEmailService`, `IStorageProvider`, `IJwtTokenService` — implementations swappable without touching Application or Domain |
| **Zero PII in logs** | Only `UserId` (opaque BIGINT) in structured logs; email/name never logged |
| **Single migration per sprint** | DB schema additions are additive-only; no column renames or drops until Phase 2 cleanup sprint |

---

## 2. Clean Architecture Layering Strategy

```
┌─────────────────────────────────────────────────────────────┐
│  API Layer  (EventManagement.API)                           │
│  • ASP.NET Core controllers — HTTP adapters only            │
│  • Middleware (CorrelationId, GlobalExceptionHandler)        │
│  • Authorization handlers & policies                        │
│  • Configuration binding (JwtSettings, SmtpSettings, etc.)  │
│  References: Application + Infrastructure                   │
├─────────────────────────────────────────────────────────────┤
│  Application Layer  (EventManagement.Application)           │
│  • MediatR Commands / Queries / Handlers                    │
│  • FluentValidation validators                              │
│  • DTOs / result types                                      │
│  • Interface contracts (IAppDbContext, IEmailService, ...)  │
│  References: Domain only                                    │
├─────────────────────────────────────────────────────────────┤
│  Domain Layer  (EventManagement.Domain)                     │
│  • Entities (User, Organization, RefreshToken, ...)         │
│  • Enums (UserRole, UserStatus, OrganizationStatus, ...)    │
│  • Domain exceptions (DomainException subclasses)           │
│  References: none (zero framework dependencies)             │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure Layer  (EventManagement.Infrastructure)     │
│  • AppDbContext (EF Core)                                   │
│  • Service implementations (JWT, email, password, consent)  │
│  • Future: Hangfire, QR, Storage, Audit                     │
│  References: Application (implements its interfaces)        │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Rule

The dependency arrows all point **inward**. Domain has zero external references. Application only references Domain. Infrastructure references Application (to implement its interfaces). API references both Application and Infrastructure (wires DI).

### MediatR CQRS Pattern

All business operations flow through MediatR:

```
Controller
  → mediator.Send(Command/Query)
    → ValidationBehavior<TRequest, TResponse>  (FluentValidation pipeline)
      → CommandHandler / QueryHandler
        → IAppDbContext / IEmailService / IJwtTokenService
          → DB / SMTP / JWT
```

This pattern keeps controllers to pure HTTP adapters, ensures validation always runs before handlers, and makes handlers independently unit-testable without HTTP context.

---

## 3. Backend Project Structure

### 3.1 Solution Layout

```
backend/
├── EventManagement.sln
├── docker-compose.yml                   ← MySQL 8 + MailHog for local dev
├── src/
│   ├── EventManagement.API/             ← Entry point (HTTP)
│   │   ├── Program.cs                   ← Bootstrap, DI, middleware pipeline
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs        ← /api/auth/* (7 endpoints, S1)
│   │   │   └── OrganizerController.cs   ← /api/organizer/* (ActiveOrganizer policy)
│   │   ├── Middleware/
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   ├── GlobalExceptionHandler.cs
│   │   │   └── Authorization/
│   │   │       ├── ActiveOrganizerHandler.cs     ← AuthorizationHandler (live DB check)
│   │   │       ├── ActiveOrganizerRequirement.cs ← IAuthorizationRequirement marker
│   │   │       └── CustomAuthorizationResultHandler.cs ← 401/403 ProblemDetails
│   │   └── Configuration/
│   │       └── CookieSettings.cs        ← SameSite: Lax (dev) / Strict (prod)
│   │
│   ├── EventManagement.Application/     ← Business logic
│   │   ├── DependencyInjection.cs       ← AddApplication() extension
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   └── ValidationBehavior.cs ← MediatR pipeline behavior
│   │   │   ├── Interfaces/
│   │   │   │   ├── IAppDbContext.cs
│   │   │   │   ├── IEmailService.cs
│   │   │   │   ├── IJwtTokenService.cs
│   │   │   │   ├── IPasswordHasher.cs
│   │   │   │   └── IConsentLedgerService.cs
│   │   │   └── Settings/
│   │   │       ├── JwtSettings.cs
│   │   │       ├── SmtpSettings.cs
│   │   │       └── FrontendSettings.cs
│   │   └── Features/
│   │       └── Auth/
│   │           ├── Login/
│   │           ├── Logout/
│   │           ├── RefreshToken/
│   │           ├── RegisterAttendee/
│   │           ├── RegisterOrganizer/
│   │           ├── ResendVerificationEmail/
│   │           └── VerifyEmail/
│   │
│   ├── EventManagement.Domain/          ← Pure domain
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Organization.cs
│   │   │   ├── RefreshToken.cs
│   │   │   ├── EmailVerificationToken.cs
│   │   │   ├── ConsentLedger.cs
│   │   │   └── LoginAttempt.cs
│   │   ├── Enums/
│   │   │   ├── UserRole.cs              ← Attendee, Organizer, CheckinStaff, SuperAdmin
│   │   │   ├── UserStatus.cs            ← PendingVerification, Active, Suspended, Deactivated
│   │   │   ├── OrganizationStatus.cs    ← PendingApproval, Active, Suspended, Rejected
│   │   │   └── ConsentType.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs       ← Base class
│   │       ├── DuplicateEmailException.cs
│   │       ├── AuthenticationFailedException.cs
│   │       ├── VerificationTokenExpiredException.cs
│   │       ├── InvalidVerificationTokenException.cs
│   │       └── InvalidRefreshTokenException.cs
│   │
│   └── EventManagement.Infrastructure/  ← I/O implementations
│       ├── DependencyInjection.cs        ← AddInfrastructure() extension
│       ├── Persistence/
│       │   └── AppDbContext.cs           ← EF Core DbContext, fluent configuration
│       ├── Migrations/
│       │   └── 20260507064901_S1_AuthCore.cs
│       └── Services/
│           ├── JwtTokenService.cs        ← HS256 access tokens + refresh token hashing
│           ├── PasswordHasher.cs         ← BCrypt work factor 12
│           ├── EmailService.cs           ← MailKit SMTP
│           └── ConsentLedgerService.cs
│
└── tests/
    ├── EventManagement.UnitTests/
    └── EventManagement.IntegrationTests/
```

### 3.2 Future Project: EventManagement.Shared

Planned for Sprint 3+. Will contain:
- IST/INR formatters
- Pagination helpers
- HMAC utility (QR token signing)
- Booking reference generator (`BK-YYYY-NNNNNN`)

**Not yet created.** (TD-02)

---

## 4. Angular Frontend Architecture

> DEV-01B not yet implemented. This section documents the intended architecture to be implemented.

### 4.1 Module Structure

```
frontend/src/app/
├── core/                          ← Singleton services, guards, interceptors
│   ├── services/
│   │   ├── auth.service.ts        ← BehaviorSubject<User>, in-memory token, login/logout
│   │   ├── api.service.ts         ← HttpClient base with environment URL
│   │   └── notification.service.ts ← MatSnackBar wrapper
│   ├── interceptors/
│   │   └── jwt.interceptor.ts     ← Attach Bearer; 401 → refresh once → redirect
│   └── guards/
│       ├── auth.guard.ts          ← isAuthenticated()
│       ├── organizer.guard.ts     ← isAuthenticated() + orgStatus=Active → /organizer/pending-approval
│       ├── admin.guard.ts
│       └── checkin-staff.guard.ts
│
├── shared/                        ← Reusable dumb components
│   └── components/
│       ├── skeleton-loader/
│       └── confirm-dialog/        ← MatDialog wrapper
│
├── public/                        ← Unauthenticated routes (lazy-loaded)
│   ├── login/
│   ├── register-attendee/         ← Reactive form, password strength meter
│   ├── register-organizer/        ← 4-step MatStepper wizard
│   ├── verify-email/              ← Token from query params, resend support
│   └── register-success/
│
├── attendee/                      ← Attendee-only routes (lazy-loaded, AuthGuard)
│   └── dashboard/
│
├── organizer/                     ← Organizer routes (lazy-loaded, OrganizerGuard)
│   ├── dashboard/
│   └── pending-approval/          ← Static page; accessible without OrganizerGuard
│
├── admin/                         ← Admin routes (lazy-loaded, AdminGuard)
│   └── dashboard/
│
└── checkin/                       ← Check-in routes (lazy-loaded, CheckinStaffGuard)
    └── events/
```

### 4.2 Authentication State Management

```
AuthService.currentUser$  (BehaviorSubject<AuthUser | null>)
  ├── accessToken: string        ← In-memory ONLY (never localStorage/sessionStorage)
  ├── userId: number
  ├── email: string
  ├── role: 'Attendee'|'Organizer'|'SuperAdmin'|'CheckinStaff'
  └── orgStatus?: string         ← Only present when role=Organizer; populated from login response
```

**Access token is memory-only.** This prevents XSS token theft. The refresh token lives in an `httpOnly` cookie and is sent automatically by the browser on `POST /api/auth/refresh`.

### 4.3 Route Guard Flow

```
Navigation → OrganizerGuard.canActivate()
  ├── Not authenticated? → /login?returnUrl=...
  ├── Authenticated, orgStatus=Active? → ✓ Proceed
  └── Authenticated, orgStatus≠Active? → /organizer/pending-approval (NOT /login)
```

### 4.4 HTTP Interceptor Flow

```
JWT Interceptor
  ├── Attach Authorization: Bearer <accessToken> to all /api/* requests
  ├── On 401 response:
  │   ├── Try POST /api/auth/refresh (once only — guard against loop)
  │   ├── On refresh success: retry original request with new token
  │   └── On refresh fail: logout + redirect to /login?returnUrl=...
  └── Attach X-Correlation-Id header (generated UUID per session)
```

### 4.5 Environment Configuration

```typescript
// environment.ts (development)
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5200/api'
};

// environment.prod.ts (production)
export const environment = {
  production: true,
  apiBaseUrl: 'https://api.yourdomain.com/api'
};
```

---

## 5. Database Architecture & Entity Relationships

### 5.1 Sprint 1 Schema (Implemented — S1_AuthCore migration)

```sql
-- Implemented via EF Core Fluent API. Actual migration: 20260507064901_S1_AuthCore

Users
  Id              BIGINT PK AUTO_INCREMENT
  Email           VARCHAR(255) UNIQUE NOT NULL      -- stored normalized (lowercase, trimmed)
  PasswordHash    VARCHAR(255) NOT NULL             -- BCrypt, work factor 12
  FullName        VARCHAR(200) NOT NULL
  Phone           VARCHAR(15) NULL
  City            VARCHAR(100) NULL
  Role            VARCHAR(20) NOT NULL              -- stored as string enum
  Status          VARCHAR(25) NOT NULL              -- stored as string enum
  CreatedAt       DATETIME(6) NOT NULL
  UpdatedAt       DATETIME(6) NOT NULL
  INDEX(Email) UNIQUE

Organizations
  Id              BIGINT PK AUTO_INCREMENT
  UserId          BIGINT UNIQUE FK → Users.Id ON DELETE CASCADE
  OrganizationName VARCHAR(200) NOT NULL
  ContactPerson   VARCHAR(200) NOT NULL
  Category        VARCHAR(100) NOT NULL
  Status          VARCHAR(25) NOT NULL              -- stored as string enum
  CreatedAt       DATETIME(6) NOT NULL
  -- S2 migration adds: Logo, Description, Address, PAN, GSTIN etc.
  INDEX(UserId) UNIQUE

EmailVerificationTokens
  Id              BIGINT PK AUTO_INCREMENT
  UserId          BIGINT FK → Users.Id ON DELETE CASCADE
  Token           VARCHAR(128) UNIQUE NOT NULL      -- cryptographically random (64 bytes → base64)
  ExpiresAt       DATETIME(6) NOT NULL              -- 24h TTL
  UsedAt          DATETIME(6) NULL
  CreatedAt       DATETIME(6) NOT NULL
  INDEX(Token) UNIQUE

RefreshTokens
  Id              BIGINT PK AUTO_INCREMENT
  UserId          BIGINT FK → Users.Id ON DELETE CASCADE
  TokenHash       VARCHAR(128) NOT NULL             -- SHA-256 hex of raw token
  ExpiresAt       DATETIME(6) NOT NULL              -- 7 days
  RevokedAt       DATETIME(6) NULL
  CreatedAt       DATETIME(6) NOT NULL
  INDEX(UserId)
  INDEX(TokenHash)

ConsentLedger
  Id              BIGINT PK AUTO_INCREMENT
  UserId          BIGINT NULL FK → Users.Id ON DELETE SET NULL
  SessionId       VARCHAR(128) NULL
  ConsentType     VARCHAR(30) NOT NULL              -- stored as string enum
  TncVersion      VARCHAR(20) NULL
  IpAddress       VARCHAR(45) NULL
  UserAgent       VARCHAR(512) NULL
  ConsentGivenAt  DATETIME(6) NOT NULL

LoginAttempts
  Id              BIGINT PK AUTO_INCREMENT
  Email           VARCHAR(255) NOT NULL
  AttemptedAt     DATETIME(6) NOT NULL
  Succeeded       TINYINT(1) NOT NULL
  IpAddress       VARCHAR(45) NULL
  INDEX(Email, AttemptedAt)
```

### 5.2 Entity Relationship (S1)

```
Users (1) ─────────── (0..1) Organizations
  │                              (UserId UNIQUE FK)
  │
  ├─── (1) ────── (0..*) EmailVerificationTokens
  ├─── (1) ────── (0..*) RefreshTokens
  └─── (1) ────── (0..*) ConsentLedger (nullable userId — pre-reg consent allowed)

LoginAttempts    (standalone — no FK; tracks by email string)
```

### 5.3 Planned Entities by Sprint

| Sprint | New Entities |
|--------|-------------|
| S2 | `OrganizationDocuments`, `AuditLog`, `PasswordResetTokens` |
| S3 | `Events`, `TicketCategories`, `EventMedia` |
| S4 | (no new tables — event lifecycle state transitions only) |
| S5 | `Bookings`, `Tickets`, `HmacQrTokens`, `Coupons`, `CouponRedemptions` |
| S6 | `EmailQueue` |
| S7 | `CheckinStaffAssignments`, `CheckinLogs` |
| S8 | `Reviews`, `ReviewFlags` |
| S9 | `DashboardSnapshots` (optional materialized) |

### 5.4 Database Conventions

- All PKs: `BIGINT AUTO_INCREMENT` (unsigned where Pomelo supports it cleanly)
- Enums stored as `VARCHAR` strings via EF Core `HasConversion<string>()` — human-readable in DB, decoupled from ordinal ordering
- All timestamps: `DATETIME(6)` in UTC; IST conversion in frontend only
- Money columns (S5+): `DECIMAL(12,2)` INR
- No soft-delete in V1 except where explicitly required (e.g., coupon deactivation)
- Migrations are additive-only per sprint — no destructive schema changes in V1

---

## 6. Authentication & Authorization Architecture

### 6.1 Identity Model

Two independent lifecycles per Organizer:

| Lifecycle | Entity | Controls |
|-----------|--------|----------|
| **User Identity** | `User.Status` | Can the user authenticate at all? |
| **Organization Business Access** | `Organization.Status` | Can the organizer perform organizer operations? |

This dual-status model is a deliberate design decision (ADR-004). An organizer with a verified email can log in and receive a JWT, but all `/api/organizer/**` endpoints are gated by the `ActiveOrganizer` policy, which performs a live DB lookup on `Organization.Status`.

### 6.2 Authorization Policy Stack

```csharp
// Registered in Program.cs
services.AddAuthorizationBuilder()
    .AddPolicy("ActiveOrganizer", policy =>
    {
        policy.RequireAuthenticatedUser();    // JWT valid
        policy.RequireRole("Organizer");       // role claim check
        policy.AddRequirements(new ActiveOrganizerRequirement()); // live DB check
    });
```

Policies planned for future sprints:

| Policy | Applies To | Requirements |
|--------|-----------|-------------|
| `ActiveOrganizer` | `/api/organizer/**` | JWT valid + role=Organizer + org.Status=Active |
| `AdminOnly` | `/api/admin/**` | JWT valid + role=SuperAdmin |
| `CheckinStaff` | `/api/checkin/**` | JWT valid + role=CheckinStaff + assignment to event |

### 6.3 ActiveOrganizerHandler — Architecture Note

The `ActiveOrganizerHandler` (implemented as a corrected version post-review) uses **live DB lookup** on every request to `/api/organizer/**`. The `ActiveOrganizerRequirement` is a marker interface with no properties.

**Key correction from review:** The original plan referenced a `Fail(new AuthorizationFailureReason(...))` call. The implemented handler correctly pairs with `CustomAuthorizationResultHandler`, which reads the failure reason message and propagates it in the 403 ProblemDetails `detail` field. This ensures the frontend receives `"Organization is pending admin approval."` — not a generic 403 message.

```csharp
// ActiveOrganizerHandler.cs — Live DB check, no caching, no stale-data risk
var org = await db.Organizations
    .AsNoTracking()
    .FirstOrDefaultAsync(o => o.UserId == userId);

if (org?.Status == OrganizationStatus.Active)
    context.Succeed(requirement);
else
    context.Fail(new AuthorizationFailureReason(this, "Organization is pending admin approval."));
```

---

## 7. JWT + Refresh Token Flow

### 7.1 Token Architecture

| Token | Type | Storage | TTL | Purpose |
|-------|------|---------|-----|---------|
| **Access Token** | JWT (HS256) | Angular service memory | 30 minutes | API authorization bearer |
| **Refresh Token** | Opaque random bytes | `httpOnly` cookie + hashed in DB | 7 days | Re-issue access token without re-login |

### 7.2 JWT Claims

```json
{
  "sub": "123",
  "email": "user@example.com",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Organizer",
  "userId": "123",
  "jti": "a1b2c3...",
  "nbf": 1746604800,
  "exp": 1746606600,
  "iss": "EventManagement",
  "aud": "EventManagement"
}
```

- `sub` = userId (string) — standard claim
- `userId` = userId (string) — convenience custom claim used by authorization handlers
- `role` = UserRole enum as string
- `jti` = unique token ID (for future token revocation support)
- `ClockSkew = TimeSpan.Zero` — no tolerance; client must refresh before expiry

### 7.3 Refresh Token Mechanics

```
POST /api/auth/login
  ├── Generate 64-byte cryptographically random raw token
  ├── SHA-256 hash → store in RefreshTokens table as TokenHash
  └── Set-Cookie: refreshToken=<raw>; HttpOnly; Secure; SameSite=Lax(dev)/Strict(prod); Path=/api/auth/refresh

POST /api/auth/refresh  (browser auto-sends cookie)
  ├── Hash incoming raw token
  ├── Lookup by TokenHash (indexed), verify ExpiresAt, RevokedAt
  ├── Rotate: set old.RevokedAt = now
  ├── Create new RefreshToken row + new raw token cookie
  └── Return new access token

POST /api/auth/logout
  ├── Set RevokedAt = now on ALL active refresh tokens for user
  └── Set-Cookie: refreshToken=; Max-Age=0 (clear cookie)
```

**Security note:** Raw token never stored; only SHA-256 hex hash. Prevents DB read → token replay attack.

### 7.4 SameSite Cookie Policy

| Environment | SameSite | Secure |
|------------|---------|--------|
| Development | `Lax` | `true` (relaxed for localhost) |
| Production | `Strict` | `true` |

Bound via `CookieSettings` class (`appsettings.Development.json` / `appsettings.Production.json`). Not hardcoded in controller logic.

---

## 8. Organizer Approval & Access Architecture

### 8.1 Lifecycle Diagram

```
Organizer Registration (US-E1-002)
  ├── User created: Role=Organizer, Status=PendingVerification
  └── Organization created: Status=PendingApproval (atomic transaction)

Email Verification (US-E1-003)
  └── User.Status → Active
      (Organization.Status remains PendingApproval)

Login (US-E1-004)
  ├── JWT issued (role=Organizer)
  └── orgStatus=PendingApproval included in login response

  ┌── Attempt to access /api/organizer/** ──────────────────────────────────┐
  │  ActiveOrganizer policy:                                                │
  │  JWT valid + role=Organizer? → Yes                                      │
  │  Organization.Status = Active? → No → HTTP 403                         │
  │  FE OrganizerGuard → /organizer/pending-approval                       │
  └─────────────────────────────────────────────────────────────────────────┘

Admin Approval (Sprint 2 — US-E2-001)
  └── POST /api/admin/organizers/{id}/approve
      Organization.Status → Active
      Trigger: approval email to organizer
      AuditLog entry: action=OrganizerApproved, actorId=adminUserId

  ┌── Organizer now accesses /api/organizer/** ──────────────────────────────┐
  │  ActiveOrganizer policy: Organization.Status = Active? → Yes → ✓         │
  └─────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Sprint 2 Additions

The following are **not yet implemented** but architecturally required for S2:
- `POST /api/admin/organizers/{id}/approve` — sets `Organization.Status=Active`, fires approval email
- `POST /api/admin/organizers/{id}/reject` — sets `Status=Rejected`, stores reason
- `POST /api/admin/organizers/{id}/suspend` — sets `Status=Suspended`
- `POST /api/admin/organizers/{id}/request-resubmission` — admin requests additional documents
- `AuditLog` entity + `IAuditWriter` service (append-only)
- Admin dashboard pending-approval queue

---

## 9. QR Generation & Validation Architecture

> Planned for Sprint 5. Architecture defined here for sprint preparation.

### 9.1 QR Token Design

QR codes are **not** simple ticket IDs. They contain an HMAC-signed payload that the backend can verify without a DB lookup on the hot check-in path.

**Token structure (Base64URL-encoded JSON payload):**

```json
{
  "ticketId": 12345,
  "bookingRef": "BK-2026-000123",
  "eventId": 99,
  "keyId": "qr-key-v1",
  "iat": 1746604800
}
```

`keyId` enables key rotation: when the HMAC secret is rotated, old tickets (signed with `qr-key-v1`) remain valid until their event passes.

**HMAC-SHA256 signature:** `HMAC(payload_json_string, QR_HMAC_SECRET)`

Full QR content: `base64url(payload) + "." + base64url(signature)`

### 9.2 QR Generation Flow (S5)

```
POST /api/bookings/confirm
  → BookingCommandHandler
    → Create Booking row
    → Create Ticket rows (one per seat)
    → For each ticket: QrCodeService.GenerateQrAsync(ticketId, bookingRef, eventId)
      → Build HMAC payload
      → Sign with QR_HMAC_SECRET (from env var)
      → Store HmacQrToken row (tokenHash, ticketId, eventId)
      → Generate PNG via QRCoder
      → Store PNG via IStorageProvider
      → Return storagePath
    → Commit transaction
    → (Post-commit) Queue booking confirmation email with QR attachment
```

### 9.3 QR Validation Flow (S7)

```
POST /api/checkin/scan
  Request: { rawQrContent: "..." }

  CheckinHandler:
    1. Decode Base64URL payload + signature
    2. Recompute HMAC(payload, QR_HMAC_SECRET_for_keyId)
    3. Compare signatures → mismatch → outcome: INVALID_SIGNATURE
    4. Parse payload: ticketId, eventId
    5. Load Ticket from DB
    6. Validate 7 outcomes:
       ├── VALID_CHECKIN     → mark ticket Checked-in, log CheckinLog
       ├── ALREADY_CHECKED_IN → show first check-in time
       ├── CANCELLED_BOOKING  → booking was cancelled
       ├── WRONG_EVENT        → ticket for different event
       ├── EVENT_NOT_STARTED  → too early (configurable window)
       ├── EVENT_ENDED        → check-in window expired
       └── INVALID_SIGNATURE  → tampered QR
```

### 9.4 HMAC Key Rotation

- `QR_HMAC_SECRET` stored in env var, referenced by `keyId` in token
- On rotation: deploy new secret with new `keyId`; old tickets remain valid during transition (both secrets active simultaneously for the transition window)
- Implementation: `IQrKeyProvider` abstraction → `EnvQrKeyProvider` (V1) → supports multiple active keys

---

## 10. Booking & Inventory Consistency Strategy

> Resolved per architectural review. PRE-06 from DEV-00 is answered here.

### 10.1 Decision: Pessimistic Row Lock (SELECT FOR UPDATE)

For V1, inventory decrement uses **MySQL pessimistic row locking** (`SELECT ... FOR UPDATE`) rather than optimistic concurrency with retry.

**Rationale:**
- Events can have burst booking (ticket release moments)
- Optimistic concurrency requires retries, which shifts load to application layer during the worst moment (burst)
- `SELECT FOR UPDATE` gives guaranteed linear throughput at the DB layer
- MySQL 8's InnoDB handles this efficiently at row level (not table level)
- V1 target load does not justify the complexity of optimistic concurrency + exponential backoff

### 10.2 Booking Transaction Boundary (S5)

```sql
BEGIN TRANSACTION;
  SELECT capacity_remaining FROM TicketCategories WHERE id=? FOR UPDATE;
  -- Abort if capacity_remaining < requested_quantity
  INSERT INTO Bookings ...
  INSERT INTO Tickets ... (one per seat)
  UPDATE TicketCategories SET capacity_remaining = capacity_remaining - ? WHERE id=?
  INSERT INTO HmacQrTokens ... (one per ticket)
  INSERT INTO AuditLog ... (booking created entry)
COMMIT;
-- POST-COMMIT (outside transaction):
  Enqueue booking confirmation email (EmailQueue)
  Enqueue PDF e-ticket generation job (Hangfire)
```

**Failure modes:**
- If DB commit fails → transaction rolls back → no booking created → return appropriate error
- If email enqueue fails post-commit → booking EXISTS, email will retry via `SmtpRetryJob`
- Booking reference (`BK-YYYY-NNNNNN`) generated inside transaction using a sequence counter per year

### 10.3 Idempotency (S5)

`POST /api/bookings/confirm` accepts `Idempotency-Key` header. If same key seen within 24h window, return cached result without re-running the transaction. This prevents double-booking from network retries.

---

## 11. Scheduler / Background Job Architecture

### 11.1 Engine: Hangfire (planned for S7)

Chosen over Quartz.NET for:
- MySQL-native persistence (no additional infrastructure)
- Built-in admin dashboard (`/hangfire` — admin-only access)
- Retry policies with exponential backoff built in
- Fire-and-forget, delayed, and recurring job types all supported

### 11.2 Job Catalog (V1)

| Job Name | Type | Schedule | Sprint | Purpose |
|----------|------|----------|--------|---------|
| `EventAutoCompleteJob` | Recurring | Every 15 min | S7 | Published → Completed when event.EndTime passed |
| `Reminder24hJob` | Recurring | Hourly | S7 | Send attendee reminders for events 24–25h ahead |
| `Reminder2hJob` | Recurring | Every 15 min | S7 | Send attendee reminders for events 2–2h15m ahead |
| `PostEventFeedbackJob` | Recurring | Hourly | S8 | Send review request 24h after event end |
| `SmtpRetryJob` | Recurring | Every 5 min | S6 | Process `EmailQueue` rows in `Failed` state (max 3 retries, exp. backoff) |
| `TicketExpiryJob` | Recurring | Hourly | S7 | Mark `Tickets.Status=Expired` for events ended >2h ago |
| `BackupJob` | Recurring | Daily 03:00 IST | S9 | Trigger `mysqldump` + compress + ship to off-host storage |

### 11.3 Hangfire Configuration (planned DependencyInjection.cs additions)

```csharp
services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
    {
        TablesPrefix = "Hangfire_"
    })));

services.AddHangfireServer(opts =>
{
    opts.WorkerCount = 2; // V1: single VPS, constrained concurrency
    opts.Queues = ["default", "email", "reports"];
});
```

### 11.4 Hangfire Dashboard Security

Dashboard mounted at `/hangfire`. Protected by custom `AdminDashboardAuthorizationFilter` that checks the JWT cookie or session — only `SuperAdmin` role can access. **Not public-facing.**

---

## 12. API Module Architecture

### 12.1 Implemented (S1 — DEV-01A)

Base URL: `/api` (no `/v1` prefix in V1; reserved for V2 API versioning introduction)

```
POST   /api/auth/register/attendee     → RegisterAttendeeCommand
POST   /api/auth/register/organizer    → RegisterOrganizerCommand
GET    /api/auth/verify-email          → VerifyEmailCommand (query param: token)
POST   /api/auth/resend-verification   → ResendVerificationEmailCommand
POST   /api/auth/login                 → LoginCommand → JWT + cookie
POST   /api/auth/refresh               → RefreshTokenCommand (cookie)
POST   /api/auth/logout                → LogoutCommand [Authorize]
GET    /api/organizer/dashboard        → OrganizerController [ActiveOrganizer policy]
```

### 12.2 Planned API Surface by Sprint

| Sprint | Module | Key Endpoints |
|--------|--------|--------------|
| S2 | Admin/Organizer Approval | `/api/admin/organizers/pending`, `/approve`, `/reject`, `/suspend` |
| S2 | User Profile | `/api/users/me`, `/api/organizers/me` |
| S3 | Events (Organizer) | `/api/events` (CRUD), `/api/events/{id}/publish` |
| S3 | Ticket Categories | `/api/events/{id}/categories` |
| S4 | Events (Public) | `/api/events` (GET), `/api/events/{publicId}` |
| S5 | Bookings | `/api/bookings/preview`, `/api/bookings/confirm`, `/api/bookings/mine` |
| S5 | Coupons | `/api/admin/coupons`, `/api/bookings/confirm` (coupon applied inline) |
| S6 | PDF + ICS | `/api/bookings/mine/{ref}/pdf`, `/api/bookings/mine/{ref}/ics` |
| S7 | Check-in | `/api/checkin/scan`, `/api/checkin/events/{id}/attendees` |
| S8 | Reviews | `/api/reviews`, `/api/admin/reviews/flagged` |
| S9 | Dashboards | `/api/organizer/dashboard/stats`, `/api/admin/dashboard/stats` |

### 12.3 API Standards

- All errors: `application/problem+json` (RFC 9457 ProblemDetails)
- Validation errors: `ValidationProblemDetails` with field-level `errors` map (HTTP 422)
- Successful list responses include pagination envelope: `{ items: [...], page, pageSize, totalCount }`
- All responses include `X-Correlation-Id` header (mirrored from request or generated)
- Future endpoints: `Idempotency-Key` header support for mutating operations (bookings, cancellations)

---

## 13. Cross-Cutting Concerns

### 13.1 Logging (Serilog)

**Library:** Serilog with `ReadFrom.Configuration` and `ReadFrom.Services`

**Output:** Structured JSON in production, human-readable template in development

**Template:**
```
[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}
```

**Enrichment:**
- `CorrelationId` — injected from `CorrelationIdMiddleware` via `LogContext.PushProperty`
- `{SourceContext}` — automatic from `ILogger<T>`

**PII Policy (strictly enforced):**
- Only `UserId` (opaque BIGINT) may appear in log messages
- Email, phone, full name, IP address — **NEVER** logged in structured logs
- Exception type and correlation ID only in error logs — no inner data

**Log Levels by Environment:**

| Environment | Default | Microsoft.AspNetCore | EF Core Commands |
|------------|---------|---------------------|-----------------|
| Development | Debug | Information | Information |
| Production | Information | Warning | Warning |

### 13.2 ProblemDetails

All error responses use RFC 9457 `application/problem+json`. Two response types:

**Standard errors (4xx/5xx):**
```json
{
  "type": "https://httpstatuses.io/409",
  "title": "Conflict",
  "status": 409,
  "detail": "Email already registered",
  "extensions": {
    "correlationId": "abc123..."
  }
}
```

**Validation errors (422):**
```json
{
  "type": "https://httpstatuses.io/422",
  "title": "Validation Failed",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "Email": ["'Email' must be a valid email address."],
    "Password": ["Password must contain at least one uppercase letter."]
  },
  "extensions": {
    "correlationId": "abc123..."
  }
}
```

### 13.3 Validation (FluentValidation)

**Pipeline behavior** (`ValidationBehavior<TRequest, TResponse>`):
- Runs synchronously before every MediatR handler
- Collects all validation errors (no short-circuit on first error)
- Throws `FluentValidation.ValidationException` if any errors → caught by `GlobalExceptionHandler` → 422 `ValidationProblemDetails`

**Key validators implemented (S1):**
- `RegisterAttendeeCommandValidator` — email format, phone 10 digits, password strength (8+ chars, uppercase, lowercase, digit, special char), TermsAccepted required
- `RegisterOrganizerCommandValidator` — same password rules + organization fields
- `LoginCommandValidator` — email format, password not empty
- `VerifyEmailCommandValidator` — token not empty

### 13.4 Exception Handling (GlobalExceptionHandler)

Implements ASP.NET Core's `IExceptionHandler` interface. Maps domain exceptions to HTTP status codes:

| Exception | HTTP Status | Title |
|-----------|------------|-------|
| `DuplicateEmailException` | 409 | Conflict |
| `VerificationTokenExpiredException` | 400 | Token Expired |
| `InvalidVerificationTokenException` | 400 | Invalid Token |
| `AuthenticationFailedException` | 401 | Unauthorized |
| `InvalidRefreshTokenException` | 401 | Unauthorized |
| `FluentValidation.ValidationException` | 422 | Validation Failed |
| Any other exception | 500 | Internal Server Error (detail sanitized) |

**Security:** 500 responses return `"An unexpected error occurred."` — never the exception message or stack trace.

### 13.5 Audit Logging (Planned — S2)

**Design:** Append-only `AuditLog` table. No UPDATE/DELETE permissions at app layer. Written via `IAuditWriter` scoped service. Every admin action (organizer approval/rejection, event closure, user suspension) creates an audit entry.

```csharp
// IAuditWriter (Application/Common/Interfaces/)
Task WriteAsync(string action, long actorId, string targetType, long targetId,
                string? detail = null, CancellationToken cancellationToken = default);
```

**S2 AuditLog table:**
```sql
AuditLog
  Id          BIGINT PK AUTO_INCREMENT
  Action      VARCHAR(100) NOT NULL    -- e.g. "OrganizerApproved", "EventCancelled"
  ActorId     BIGINT NOT NULL          -- UserId of the person who did the action
  TargetType  VARCHAR(50)              -- e.g. "Organization", "Event"
  TargetId    BIGINT
  Detail      TEXT NULL                -- JSON blob for extra context
  OccurredAt  DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP
  INDEX(ActorId)
  INDEX(TargetType, TargetId)
```

### 13.6 Correlation ID Middleware

`CorrelationIdMiddleware.cs` (implemented):
- Reads `X-Correlation-Id` request header; generates a new GUID if absent
- Writes value to response header
- Pushes to Serilog `LogContext` — all downstream log entries in the request scope carry it
- Angular JWT interceptor should also attach `X-Correlation-Id` (client-generated) to enable end-to-end tracing

---

## 14. Environment & Configuration Architecture

### 14.1 Configuration Hierarchy

```
appsettings.json                   ← Base config (non-secret values only)
  └── overridden by
appsettings.{Environment}.json     ← Dev/Staging/Prod overrides
  └── overridden by
Environment variables              ← Secrets: JWT_SECRET, DB password, SMTP credentials
```

**Rule:** No secrets in any `appsettings.*.json` file. No secrets committed to source control.

### 14.2 Configuration Sections

| Section | Class | Source |
|---------|-------|--------|
| `Jwt` | `JwtSettings` | `appsettings.json` (Issuer, Audience, TTLs); `JWT_SECRET` from env var only |
| `Smtp` | `SmtpSettings` | Dev: MailHog localhost:1025; Prod: env vars |
| `Cookie` | `CookieSettings` | `SameSite=Lax` in dev.json; `SameSite=Strict` in prod.json |
| `Frontend` | `FrontendSettings` | BaseUrl for email link generation |
| `AllowedOrigins` | `string[]` | CORS; dev: `http://localhost:4200` |
| `ConnectionStrings:DefaultConnection` | — | Dev: dev.json; Prod: env var |

### 14.3 Mandatory Environment Variables (Production)

| Variable | Purpose |
|----------|---------|
| `JWT_SECRET` | HS256 signing key (≥256-bit / 32 chars minimum) |
| `ConnectionStrings__DefaultConnection` | MySQL connection string |
| `Smtp__Host` | SMTP provider host |
| `Smtp__Username` | SMTP auth username |
| `Smtp__Password` | SMTP auth password |
| `QR_HMAC_SECRET` | QR token signing key (planned for S5) |

**Startup guard:** `Program.cs` throws `InvalidOperationException` on startup if `JWT_SECRET` is missing. This prevents silent misconfiguration in production.

### 14.4 Local Development Setup

```bash
# Start dependencies
docker compose up -d   # MySQL 8 on :3306, MailHog on :1025/:8025

# Set required secret
$env:JWT_SECRET = "this-is-my-local-dev-secret-key-for-event-platform-2026"

# Run API
dotnet run --project src/EventManagement.API --launch-profile http

# View captured emails
# http://localhost:8025
```

---

## 15. Deployment Architecture

### 15.1 V1 Single-VPS Topology

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                     Production VPS (e.g. Hetzner CX21 / Digital Ocean)       │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │  nginx (Docker)                                                         │ │
│  │  ├── TLS termination (Let's Encrypt via certbot/acme.sh sidecar)        │ │
│  │  ├── /api/** → proxy_pass → ASP.NET Core container :5200                │ │
│  │  ├── /hangfire → proxy_pass → ASP.NET Core :5200/hangfire (admin only)  │ │
│  │  └── / → /usr/share/nginx/html (Angular static build)                  │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                               │
│  ┌──────────────────────────────────┐  ┌───────────────────────────────────┐ │
│  │  ASP.NET Core API (Docker)       │  │  Hangfire Worker (same image      │ │
│  │  eventmanagement-api:latest      │  │  or separate — shares DB/DI)      │ │
│  │  :5200 (internal)                │  │  :5201 (internal, optional)        │ │
│  └──────────────────────────────────┘  └───────────────────────────────────┘ │
│                                                                               │
│  ┌──────────────────────────────────┐                                         │
│  │  MySQL 8.x (Docker)              │                                         │
│  │  Volume: /var/app/mysql-data      │                                         │
│  └──────────────────────────────────┘                                         │
│                                                                               │
│  /var/app/uploads  (volume-mounted into API container for IStorageProvider)  │
│  /var/app/backups  (daily mysqldump output, rotated 7 days)                  │
└──────────────────────────────────────────────────────────────────────────────┘
         │                               │
         ▼                               ▼
   External SMTP               Daily backup → off-host storage
   (Brevo / SendGrid           (e.g. Backblaze B2 or rsync to second VPS)
    free tier)
```

### 15.2 Docker Compose (Production)

A separate `docker-compose.prod.yml` (not yet created — TD-03) with:
- `api` service: `env_file: .env.prod` with all secrets
- `mysql` service: no exposed ports (internal only)
- `nginx` service: certbot/acme volumes
- Named volumes for data persistence
- `restart: unless-stopped` on all services

### 15.3 CI/CD Pipeline (GitHub Actions)

| Trigger | Workflow | Steps |
|---------|----------|-------|
| PR opened | `ci-backend.yml` | `dotnet restore` → `dotnet build` → `dotnet test` → security scan (`dotnet list package --vulnerable`) |
| PR opened | `ci-frontend.yml` | `npm ci` → `ng build` → `ng test --watch=false` |
| Merge to `develop` | `deploy-staging.yml` | Build image → push registry → SSH deploy to staging VPS |
| Git tag (`v*.*.*`) | `deploy-prod.yml` | Build image → push registry → manual approval gate → deploy to prod |

**EF Core migrations:** Applied automatically at startup via `context.Database.MigrateAsync()` in a startup `IHostedService`. Transactional migration history prevents partial-apply.

---

## 16. Security Architecture

### 16.1 OWASP Top 10 Coverage

| Risk | Mitigation |
|------|-----------|
| **A01 — Broken Access Control** | Authorization policies per endpoint (`[Authorize(Policy="ActiveOrganizer")]`). All organizer/admin endpoints explicitly policy-gated. OrganizerGuard does live DB check — no stale JWT claims. |
| **A02 — Cryptographic Failures** | BCrypt work factor 12 for passwords. JWT secret ≥256-bit from env var. Refresh token: raw never stored; SHA-256 hex hash in DB. HTTPS enforced. |
| **A03 — Injection** | EF Core parameterized queries exclusively. FluentValidation before any handler processes input. Angular auto-escapes template bindings. |
| **A04 — Insecure Design** | Dual-status Organizer model prevents access via JWT role alone. No payment data in V1 (reduces PCI scope). Booking reference generation server-side only. |
| **A05 — Security Misconfiguration** | CORS restricted to configured `AllowedOrigins`. `httpOnly`+`Secure` refresh cookie. No Swagger in production. Startup guard for missing `JWT_SECRET`. |
| **A06 — Vulnerable Components** | CI pipeline runs `dotnet list package --vulnerable` on every PR. |
| **A07 — Auth Failures** | No user enumeration: resend-verification returns 200 regardless of email existence. No enumeration: same 401 for wrong password and unknown email. Account lockout (S1 companion stories US-E1-006). |
| **A08 — Software & Data Integrity** | Migrations version-controlled. Docker images pinned to digest in CI. |
| **A09 — Logging & Monitoring** | Correlation ID on every request. Structured logging via Serilog. LoginAttempts table for audit. No PII in logs. |
| **A10 — SSRF** | No outbound HTTP from API in V1 (SMTP via MailKit only). File upload validation (planned S3+). |

### 16.2 Security Headers (planned nginx config — S9)

```nginx
add_header X-Frame-Options "DENY";
add_header X-Content-Type-Options "nosniff";
add_header Referrer-Policy "strict-origin-when-cross-origin";
add_header Permissions-Policy "camera=(), microphone=(), geolocation=()";
add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;";
```

### 16.3 Rate Limiting (planned S2)

ASP.NET Core built-in rate limiter (`Microsoft.AspNetCore.RateLimiting`):

| Endpoint Group | Policy | Limit |
|---------------|--------|-------|
| `POST /api/auth/login` | Fixed window | 10 req / 1 min per IP |
| `POST /api/auth/register/**` | Fixed window | 5 req / 1 min per IP |
| `POST /api/auth/resend-verification` | Token bucket | 3 req / 15 min per IP |
| `POST /api/bookings/confirm` | Sliding window | 5 req / 1 min per userId |
| General API | Sliding window | 200 req / 1 min per IP |

---

## 17. Architecture Decision Records (ADRs)

### ADR-001: Modular Monolith over Microservices for V1

**Status:** Accepted  
**Context:** Single developer / small team, V1 launch, free infrastructure budget, Angular + ASP.NET Core stack locked in.  
**Decision:** Single deployable API. Feature modules (Auth, Events, Bookings, etc.) isolated by namespace/folder within the monolith. `IEmailService` and `IStorageProvider` interfaces kept clean for future extraction.  
**Consequences:** Simpler deployment, simpler local dev, no distributed tracing needed. Trade-off: module coupling must be watched as codebase grows. Rule of Three before extraction.  

---

### ADR-002: MediatR CQRS-Lite over Service Classes

**Status:** Accepted  
**Context:** Need consistent validation, auditing, and testability across all operations.  
**Decision:** All mutations via MediatR `ICommand`/`ICommandHandler`. Validation via `IPipelineBehavior`. Controllers are pure HTTP adapters.  
**Consequences:** Uniform pipeline for all operations. Handlers are independently unit-testable. Slight ceremony for trivial operations but pays off in consistency. Every handler has a co-located validator.  

---

### ADR-003: MySQL 8 with Pessimistic Row Lock for Booking Inventory

**Status:** Accepted  
**Context:** Booking confirm must decrement ticket category capacity atomically. PRE-06 from DEV-00.  
**Decision:** `SELECT ... FOR UPDATE` on `TicketCategories` row inside booking transaction. No optimistic concurrency with retries.  
**Consequences:** Deterministic behavior under burst. Slight reduction in parallelism (lock contention) — acceptable for V1 load profile. Optimistic path deferred to Phase 2 if profiling shows bottleneck.  

---

### ADR-004: Dual-Status Organizer Model (User.Status + Organization.Status)

**Status:** Accepted  
**Context:** Organizer must be able to log in (verify JWT identity) before admin approval, but must be blocked from organizer operations until approved.  
**Decision:** Two independent lifecycle states. JWT carries `role=Organizer` but does NOT carry `Organization.Status`. `ActiveOrganizer` policy performs live DB lookup on every request.  
**Consequences:** No stale authorization data — admin approval/suspension takes effect on the next request. Small DB overhead per organizer request (single indexed lookup). Correct security posture.  

---

### ADR-005: Refresh Token as httpOnly Cookie (Not Bearer Token)

**Status:** Accepted  
**Context:** Refresh token must survive page reload but must not be accessible to JavaScript (XSS mitigation).  
**Decision:** Refresh token stored in `httpOnly; Secure; SameSite` cookie. Raw token never stored in DB; SHA-256 hash stored. Access token in Angular service memory only.  
**Consequences:** Refresh token cannot be stolen via XSS. CSRF risk mitigated by `SameSite=Strict` in production. Dev uses `SameSite=Lax` to work with localhost cross-origin.  

---

### ADR-006: String Enum Storage in MySQL

**Status:** Accepted  
**Context:** EF Core supports both integer and string enum storage.  
**Decision:** All enums stored as `VARCHAR` strings via `HasConversion<string>()`. Never as integers.  
**Consequences:** Human-readable values directly in DB (easier debugging, migrations, support queries). No risk of silent ordinal mismatch when enums are reordered. Slight storage overhead — acceptable.  

---

### ADR-007: No API Versioning Prefix (/v1) in V1

**Status:** Accepted  
**Context:** Planning artifacts suggested `/api/v1` prefix. Decision made during implementation.  
**Decision:** Base URL is `/api` with no version segment in V1. Versioning strategy (URL-segment vs header) deferred to Phase 2.  
**Consequences:** Simpler routes in V1. V2 introduction requires a migration strategy. Mitigated: Angular services can be updated centrally. No external public consumers in V1.  

---

### ADR-008: EventManagement.Shared Project Deferred

**Status:** Deferred — Technical Debt TD-02  
**Context:** IST/INR formatters, HMAC utils, booking reference generator could be in a shared project.  
**Decision:** These utilities will be implemented inline in Infrastructure/Application for S1-S4. `EventManagement.Shared` created before S5 (booking reference generator is needed).  
**Consequences:** Some minor duplication in early sprints. No cross-cutting impact — utilities are pure functions, easy to move.  

---

## 18. Technical Debt Register

| ID | Description | Impact | Target Resolution |
|----|-------------|--------|------------------|
| **TD-01** | `dotnet ef migrations` command must be run from `backend/src` subfolder (not `backend/`), contrary to some doc examples. The `--startup-project` flag requires the correct relative path. | Low — developer friction | Update all runbook references before S2 |
| **TD-02** | `EventManagement.Shared` project not yet created. IST/INR formatters, HMAC utils, pagination helpers, booking reference generator will be added inline until S5. | Medium — some utility duplication | Create project before Sprint 5 (required for booking reference generator) |
| **TD-03** | `docker-compose.prod.yml` does not yet exist. Production deployment uses a manual procedure. | High — deployment risk | Create before Staging environment setup (recommended: before S3 deploy) |
| **TD-04** | No rate limiting middleware configured yet. All auth endpoints are currently unthrottled. Login brute force is partially mitigated by lockout (US-E1-006, S1 companion) but no IP-level throttle exists. | Medium-High — security gap | Implement in Sprint 2 as part of hardening |
| **TD-05** | `appsettings.json` contains a default MySQL connection string with `root/root` credentials. This is a dev convenience but could mislead a production deployment if env vars are not overridden. | Medium — misconfiguration risk | Replace with an obviously invalid placeholder (`"REPLACE_WITH_ENV_VAR"`) before S3 |
| **TD-06** | `OrganizerController.cs` contains only a stub dashboard endpoint. Full organizer API surface not yet implemented. | Low — expected gap at S1 | Progressively filled in S2/S3/S4 |

---

## 19. Future Scalability Guidance

### 19.1 Horizontal API Scaling (Phase 2)

The API is stateless by design. To scale horizontally:
1. Add a load balancer (nginx upstream block or cloud LB)
2. Sticky sessions **not needed** — JWT is self-contained
3. `httpOnly` refresh cookie: ensure `Path=/api/auth/refresh` is consistent across all nodes
4. Hangfire: configure a shared MySQL backend (already true) — workers can run on any node

### 19.2 Read Replicas (Phase 2)

EF Core `AddDbContext` can be extended to use a separate read-only `DbContext` for query handlers. Design:
```csharp
// Add in DependencyInjection.cs when read replica available:
services.AddDbContext<ReadOnlyAppDbContext>(options =>
    options.UseMySql(readReplicaConnectionString, ...));
services.AddScoped<IReadOnlyAppDbContext>(sp => sp.GetRequiredService<ReadOnlyAppDbContext>());
```
Query handlers use `IReadOnlyAppDbContext`; command handlers use `IAppDbContext`.

### 19.3 Caching (Phase 2)

No Redis in V1. Design is cache-friendly:
- Event listings are indexed and paginated — can be cached at nginx or Redis with short TTL
- Organization status is intentionally NOT cached (authorization correctness requirement)
- Add `IDistributedCache` abstraction in Application layer before implementing; swap `InMemoryCache` → `RedisCache` without handler changes

### 19.4 Notification Channel Expansion (Phase 2)

Current: `IEmailService` → `EmailService` (SMTP via MailKit)

Phase 2 additions:
```csharp
// INotificationChannel (Application/Common/Interfaces/)
Task SendAsync(NotificationMessage message, CancellationToken cancellationToken);

// Implementations (Infrastructure/Notifications/)
SmtpEmailChannel   // current EmailService promoted
SmsChannel         // Twilio / MSG91
PushChannel        // Firebase FCM
```

### 19.5 Storage Provider Expansion (Phase 2)

Current: `IStorageProvider` → `LocalFileStorageProvider`

```csharp
// Phase 2 swap — no Application/Domain changes needed:
S3StorageProvider       // AWS S3 / Backblaze B2
CloudflareR2Provider    // Cloudflare R2 (free egress)
```

### 19.6 Microservices Extraction Candidates (Phase 3)

| Candidate | Reason | Extraction Effort |
|-----------|--------|------------------|
| Notification Service | High volume, independent scaling | Low — `IEmailService` interface already isolated |
| Check-in Service | Edge deployment (offline-capable), independent load profile | Medium — needs QR key distribution |
| Reporting / Dashboard Service | Read-heavy, separate scaling profile | Medium — needs read replica access |
| Identity Service | Shared across products | High — requires cross-service JWT issuer |

---

## 20. Sprint 5 Booking-Locking Architectural Preparation

> PRE-06 from DEV-00 is resolved. This section provides the full technical brief for the Sprint 5 implementor.

### 20.1 Pre-conditions for Sprint 5

- [ ] `EventManagement.Shared` project created (TD-02)
- [ ] `Bookings`, `Tickets`, `HmacQrTokens` tables designed and migration drafted
- [ ] `IQrCodeService` interface defined in Application layer
- [ ] `QrCodeService` implementation (QRCoder library) added to Infrastructure
- [ ] `IStorageProvider` finalized and `LocalFileStorageProvider` implemented
- [ ] `EmailQueue` table and `SmtpRetryJob` designed (S6 prerequisite)
- [ ] `QR_HMAC_SECRET` env var documented in deployment runbook

### 20.2 Booking Command Architecture

```
POST /api/bookings/confirm
  Request: { eventId, categoryId, quantity, couponCode?, idempotencyKey }
  Headers: Idempotency-Key: <uuid>

BookingCommandHandler:
  1. Check IdempotencyKey cache (24h window) — return cached result if hit
  2. BEGIN TRANSACTION
     a. SELECT ... FOR UPDATE on TicketCategories WHERE id=@categoryId
     b. Validate capacity_remaining >= quantity
     c. Validate event.Status = Published, event.BookingWindowOpen
     d. Apply coupon if provided (validate + decrement CouponRedemptions)
     e. INSERT Bookings (ref=BK-YYYY-NNNNNN, status=Confirmed)
     f. INSERT Tickets (quantity rows, status=Issued)
     g. For each ticket: generate HMAC QR payload + sign
     h. INSERT HmacQrTokens
     i. UPDATE TicketCategories SET capacity_remaining -= quantity
     j. INSERT AuditLog (BookingCreated)
  3. COMMIT
  4. Post-commit (fire-and-forget via Hangfire):
     a. Enqueue confirmation email job
     b. Enqueue PDF e-ticket generation job
  5. Return BookingResult { bookingRef, tickets[{id, qrImageUrl}] }
```

### 20.3 Concurrency Test Requirement

Before Sprint 5 merges, a **load test** must confirm:
- 50 concurrent booking requests for a ticket category with capacity=10
- Result: exactly 10 bookings confirmed, 40 rejected with `SOLD_OUT`
- No race conditions observed (verified by row count in DB post-test)

**Test file:** `backend/tests/EventManagement.LoadTests/BookingConcurrencyTests.cs` (to be created in S5)

### 20.4 Booking Reference Generation

```csharp
// In EventManagement.Shared (TD-02 must be resolved before S5)
public static class BookingReferenceGenerator
{
    // BK-2026-000001 — year-scoped, zero-padded 6 digits
    // Counter stored in DB: BookingCounters table (id=year, counter INT)
    // Incremented atomically inside booking transaction using:
    // INSERT INTO BookingCounters (year, counter) VALUES (2026, 1)
    //   ON DUPLICATE KEY UPDATE counter = counter + 1;
    // SELECT counter FROM BookingCounters WHERE year=2026;
    public static string Format(int year, long counter) =>
        $"BK-{year}-{counter:D6}";
}
```

---

*Document end. Version: 1.0.0 | Review cadence: After each sprint that introduces new architecture decisions.*
