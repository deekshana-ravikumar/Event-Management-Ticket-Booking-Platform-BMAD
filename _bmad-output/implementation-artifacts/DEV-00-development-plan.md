# DEV-00 — Development Execution Plan

**Project:** Smart Event Management & Ticket Booking Platform
**Author:** Amelia (Senior Software Engineer)
**Date:** 2026-05-06
**Status:** Planning — Pre-implementation
**Sprint Plan Source:** pm-05-sprint-plan.md
**Story Source:** pm-04-product-backlog.md

---

## 1. Technical Implementation Sequence

Canonical dependency chain (from PM-05):

```
Auth + JWT + SMTP infra
  └─► Organizer approval workflow + Audit log
        └─► Event authoring + Ticket categories + Publish
              └─► Public discovery + Search
                    └─► Booking (atomic, instant-confirm, BR-NEW-01 cap)
                          └─► HMAC QR generation
                                └─► Email + PDF e-ticket (QuestPDF)
                                      └─► Check-in scanner (7 outcomes)
                                            └─► Self-cancel + Lifecycle emails
                                                  └─► Reviews + Re-review
                                                        └─► Dashboards + CSV + Compliance
```

This is a strict waterfall-of-concerns within an iterative sprint model. **No shortcutting.** Booking cannot start without atomic DB locking pattern reviewed by architecture. QR cannot exist without Booking. Check-in cannot exist without QR.

---

## 2. Technical Implementation Sequence — Sprint Mapping

| Phase | Sprint | Domain | Unblocking Deliverable |
|-------|--------|--------|------------------------|
| **P1** | S1 | Auth + Platform skeleton + DPDP | JWT issuer, SMTP sender, /health |
| **P2** | S2 | Identity completion + Org governance + Audit | Organizer active, audit log live |
| **P3** | S3 | Event authoring + Ticket categories | Publishable event exists |
| **P4** | S4 | Event lifecycle + Public discovery | Attendee can find events |
| **P5** | S5 | Booking core + QR token | Ticket in hand |
| **P6** | S6 | Booking UX + Coupons + Email + PDF | Full attendee experience |
| **P7** | S7 | Check-in scanner + Manual lookup + Scheduler | Gate control operational |
| **P8** | S8 | Self-cancel + Reviews + Sensitive re-review + Emails | Trust layer complete |
| **P9** | S9 | Dashboards + Compliance + Hardening + Beta | GA-ready |

---

## 3. Story 1 Candidate — Recommended

### US-E1-001 + US-E1-002 + US-E1-003 + US-E1-004 (Sprint 1 Auth Core)

These four stories form the **unbreakable atomic unit** of the platform. Nothing else exists without them. Recommend implementing as a single "Story 1" super-story, but tracking each AC set independently.

**Recommended Story 1 Package:**

| Story ID | Title | SP |
|----------|-------|----|
| US-E1-001 | Attendee registration | 3 |
| US-E1-002 | Organizer registration | 3 |
| US-E1-003 | Email verification | 3 |
| US-E1-004 | Login (JWT issuance + refresh) | 5 |
| **Total** | | **14 SP** |

**Rationale:** These are zero-dep stories (no upstream dependency). They produce the auth primitives used by every subsequent story. Cannot split and still have a runnable system to build on.

**Immediate follow-on** (same sprint, start after Story 1 merges):

| Story ID | Title | SP |
|----------|-------|----|
| US-E1-005 | Forgot password | 3 |
| US-E1-006 | Account lockout (5 attempts) | 2 |
| US-E1-007 | Session timeout 30 min | 2 |
| US-E7-001 | Verification email | 2 |
| US-E7-008 | SMTP retry (3× backoff) | 5 |
| US-E10-003 | /health endpoint | 2 |
| US-E10-005 | IST + INR formatters | 3 |
| US-E10-008 | DPDP consent ledger | 3 |

---

## 4. Project Scaffolding Plan

### 4.1 Repository Structure

```
/
├── backend/                          # ASP.NET Core Web API
│   ├── src/
│   │   ├── EventManagement.API/      # Entry point — controllers, middleware, DI
│   │   ├── EventManagement.Domain/   # Entities, enums, value objects, domain events
│   │   ├── EventManagement.Application/  # MediatR handlers, DTOs, FluentValidation
│   │   ├── EventManagement.Infrastructure/  # EF Core, repositories, SMTP, storage, QR
│   │   └── EventManagement.Shared/   # Cross-cutting: IST/INR utils, HMAC, pagination
│   └── tests/
│       ├── EventManagement.UnitTests/
│       ├── EventManagement.IntegrationTests/
│       └── EventManagement.LoadTests/
│
├── frontend/                         # Angular application
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                 # Guards, interceptors, auth service, models
│   │   │   ├── shared/               # Shared components (EventCard, ConfirmDialog…)
│   │   │   ├── public/               # PublicModule — lazy-loaded
│   │   │   ├── attendee/             # AttendeeModule — lazy-loaded
│   │   │   ├── organizer/            # OrganizerModule — lazy-loaded
│   │   │   ├── admin/                # AdminModule — lazy-loaded
│   │   │   └── checkin/              # CheckinModule — lazy-loaded
│   │   ├── assets/
│   │   └── environments/
│   └── ...
│
├── docs/                             # Architecture + API + dev runbook
├── _bmad-output/                     # Planning + implementation artifacts
│   ├── planning-artifacts/
│   └── implementation-artifacts/
│
├── .github/
│   └── workflows/
│       ├── ci-backend.yml
│       └── ci-frontend.yml
│
├── docker-compose.yml                # Local dev: MySQL 8 + MailHog SMTP
└── README.md
```

### 4.2 Backend Project Setup Checklist

| # | Task | Tool/Package |
|---|------|-------------|
| B-01 | Create solution + 5 projects | `dotnet new sln`, `dotnet new webapi/classlib` |
| B-02 | Add project references (API → Application → Domain; Infrastructure → Application) | `dotnet add reference` |
| B-03 | Add NuGet packages to Application | MediatR 12, FluentValidation.AspNetCore, AutoMapper |
| B-04 | Add NuGet packages to Infrastructure | EF Core 8, Pomelo.EntityFrameworkCore.MySql, Hangfire.MySql, QuestPDF, MailKit, QRCoder, Serilog |
| B-05 | Add NuGet packages to API | Swashbuckle, Microsoft.AspNetCore.Authentication.JwtBearer, Serilog.AspNetCore |
| B-06 | Configure EF Core DbContext with Pomelo MySQL driver | `UseMySql(connString, ServerVersion.AutoDetect)` |
| B-07 | Configure Serilog with structured JSON output + correlation ID middleware | `appsettings.json` sinks |
| B-08 | Configure JWT authentication + refresh token pattern | `AddAuthentication(JwtBearer)` |
| B-09 | Configure CORS for Angular dev origin | `localhost:4200` → dev only |
| B-10 | Configure Swagger with JWT Bearer scheme | `AddSecurityDefinition` |
| B-11 | Add `IStorageProvider` abstraction + `LocalStorageProvider` impl | Interface in Application, impl in Infrastructure |
| B-12 | Configure FluentValidation pipeline behaviour | `ValidationBehavior<TRequest, TResponse>` |
| B-13 | Set up Hangfire with MySQL persistence + dashboard (admin-only) | `AddHangfire`, `AddHangfireServer` |
| B-14 | Seed: Super Admin account (env-var driven, never hardcoded) | DB migration seeder |
| B-15 | Configure `appsettings.Development.json` for local MySQL + MailHog | `ConnectionStrings`, `Smtp`, `Jwt`, `Storage` |

### 4.3 Frontend Project Setup Checklist

| # | Task | Command/Tool |
|---|------|-------------|
| F-01 | Create Angular 17+ project with standalone components enabled | `ng new frontend --routing --style=scss` |
| F-02 | Add Angular Material | `ng add @angular/material` (theme: Indigo/Pink → override with brand tokens) |
| F-03 | Add lazy-loaded feature modules | `ng g module public/public --routing`, repeat for attendee/organizer/admin/checkin |
| F-04 | Configure HTTP interceptors: JWT attach, 401 redirect, correlation ID header | `core/interceptors/` |
| F-05 | Configure Angular route guards | `AuthGuard`, `OrganizerGuard`, `AdminGuard`, `CheckinStaffGuard` in `core/guards/` |
| F-06 | Configure Angular environments | `environment.ts` (API base URL, prod flag) |
| F-07 | Add `html5-qrcode` library | `npm install html5-qrcode` |
| F-08 | Configure IST date pipe + INR currency pipe in SharedModule | `DateTimeDisplayComponent`, `CurrencyDisplayComponent` |
| F-09 | Configure `HttpClientModule` + API service base class | `core/services/api.service.ts` |
| F-10 | Configure `MatSnackBar` wrapper service for toast notifications | `core/services/notification.service.ts` |
| F-11 | Add PWA support (for check-in scanner) | `ng add @angular/pwa` |
| F-12 | Configure SCSS global design tokens (colours, spacing, typography) | `styles/tokens.scss` |
| F-13 | Add skeleton loader component to SharedModule | `shared/components/skeleton-loader/` |
| F-14 | Add `ConfirmDialogComponent` to SharedModule (MatDialog wrapper) | `shared/components/confirm-dialog/` |

### 4.4 Database Setup Checklist

| # | Task | Notes |
|---|------|-------|
| D-01 | MySQL 8 instance | Local install (recommended for dev) **or** Docker: `docker compose up -d` in `backend/` |
| D-02 | Create database: `eventmanagement_dev` | See §4.6 Local Development Setup below |
| D-03 | Create EF Core S1 migration | `dotnet ef migrations add S1_AuthCore --project EventManagement.Infrastructure --startup-project EventManagement.API` (run from `backend/` — adjust to `src/EventManagement.*` if your solution uses a `/src` subfolder) |
| D-04 | Plan V1 schema (see §4.5) | Run migration before Story 1 coding |
| D-05 | Configure connection string per env via env vars only | Never commit credentials |
| D-06 | Set up Hangfire schema (created automatically on first run) | — |

### 4.5 Core Database Schema (V1 — Story 1 Scope: Auth tables + minimal `Organizations`)

```sql
-- Users table (Story 1 target)
Users
  Id              BIGINT UNSIGNED AUTO_INCREMENT PK
  Email           VARCHAR(255) UNIQUE NOT NULL
  PasswordHash    VARCHAR(255) NOT NULL          -- BCrypt
  FullName        VARCHAR(200) NOT NULL
  Phone           VARCHAR(15)
  City            VARCHAR(100)
  Role            ENUM('Attendee','Organizer','CheckinStaff','SuperAdmin') NOT NULL
  Status          ENUM('PendingVerification','Active','Suspended','Deactivated') NOT NULL DEFAULT 'PendingVerification'
  CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
  UpdatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP

-- EmailVerificationTokens (Story US-E1-003)
EmailVerificationTokens
  Id              BIGINT UNSIGNED AUTO_INCREMENT PK
  UserId          BIGINT UNSIGNED FK → Users.Id
  Token           VARCHAR(128) UNIQUE NOT NULL   -- cryptographically random, NOT guessable
  ExpiresAt       DATETIME NOT NULL              -- +24h from issue
  UsedAt          DATETIME NULL
  CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP

-- RefreshTokens (Story US-E1-004)
RefreshTokens
  Id              BIGINT UNSIGNED AUTO_INCREMENT PK
  UserId          BIGINT UNSIGNED FK → Users.Id
  TokenHash       VARCHAR(128) NOT NULL          -- SHA-256 hash of the token
  ExpiresAt       DATETIME NOT NULL
  RevokedAt       DATETIME NULL
  CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
  INDEX(UserId)

-- ConsentLedger (US-E10-008 — DPDP baseline, Story 1)
ConsentLedger
  Id              BIGINT UNSIGNED AUTO_INCREMENT PK
  UserId          BIGINT UNSIGNED FK → Users.Id NULL  -- NULL = pre-registration consent
  SessionId       VARCHAR(128)
  ConsentType     ENUM('Registration','TermsAcceptance','BookingTnC','CookieBanner') NOT NULL
  TncVersion      VARCHAR(20)                    -- e.g. "2026-05-01"
  IpAddress       VARCHAR(45)                    -- IPv4/IPv6
  UserAgent       VARCHAR(512)
  ConsentGivenAt  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP

-- LoginAttempts (US-E1-006 — lockout)
LoginAttempts
  Id              BIGINT UNSIGNED AUTO_INCREMENT PK
  Email           VARCHAR(255) NOT NULL
  AttemptedAt     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
  Succeeded       TINYINT(1) NOT NULL
  IpAddress       VARCHAR(45)
  INDEX(Email, AttemptedAt)

-- Organizations (US-E1-002 — minimal; full org fields added in S2 migration)
-- Locked into S1_AuthCore migration: organizer registration writes User + Organization atomically.
Organizations
  Id              BIGINT UNSIGNED AUTO_INCREMENT PK
  UserId          BIGINT UNSIGNED UNIQUE FK → Users.Id  -- one org per user
  OrganizationName VARCHAR(200) NOT NULL
  ContactPerson   VARCHAR(200) NOT NULL
  Category        VARCHAR(100) NOT NULL
  Status          ENUM('PendingApproval','Active','Suspended','Rejected') NOT NULL DEFAULT 'PendingApproval'
  CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
  INDEX(UserId)
  -- Additional columns (Logo, Description, Address, PAN, GSTIN etc.) added in S2 migration
```

**Tables deferred to later sprints (not in Story 1 migration):**
```
S2: OrganizationDocuments, AuditLog (Organizations table minimal-version is in S1; S2 adds columns)
S3: Events, TicketCategories, EventMedia
S5: Bookings, Tickets, HmacQrTokens
S5: Coupons, CouponRedemptions
S6: EmailQueue
S7: CheckinStaff, CheckinLogs
S8: Reviews
S9: DashboardSnapshots (optional materialized view)
```

### 4.6 Local Dev Environment (docker-compose.yml plan)

```yaml
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
      MYSQL_DATABASE: eventmgmt_dev
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  mailhog:
    image: mailhog/mailhog
    ports:
      - "1025:1025"   # SMTP
      - "8025:8025"   # Web UI

volumes:
  mysql_data:
```

SMTP config for dev: `host=localhost, port=1025, no auth`. MailHog captures all outgoing emails — visible at `http://localhost:8025`.

---

## 5. Dependency Prerequisites (Must Resolve Before Coding Starts)

| # | Prerequisite | Owner | Blocker for |
|---|-------------|-------|------------|
| PRE-01 | MySQL 8 local instance running (docker-compose up) | Dev | All backend stories |
| PRE-02 | `.env` / `appsettings.Development.json` with valid local connection string | Dev | S1 BE work |
| PRE-03 | MailHog running; SMTP config pointing to `localhost:1025` | Dev | US-E1-003, US-E7-001 |
| PRE-04 | JWT secret ≥256-bit, stored in env var (not appsettings) | Dev | US-E1-004 |
| PRE-05 | CI pipeline (GitHub Actions or similar) with: dotnet test + ng test | Dev | Every story DoD |
| PRE-06 | **Architect sign-off on DB locking pattern** (select-for-update vs optimistic concurrency) | Architect | **S5 US-E5-005** |
| PRE-07 | Hangfire DB schema provisioned (can be done in S1 migration as no-op tables) | Dev | S7 US-E3-011 |
| PRE-08 | `IStorageProvider` interface reviewed and agreed (local for V1; interface stable for V2 cloud swap) | Dev | S2 US-E2-001 |
| PRE-09 | Angular Material theme tokens committed to `styles/tokens.scss` before any FE story | FE | All FE stories |
| PRE-10 | Super Admin seed credentials in `.env` (not hardcoded) | Dev | S2 admin stories |

---

## 6. Story 1 Specification (Executable)

Story 1 file written to: `DEV-01-story-auth-core.md`

See that file for full AC mapping, file-level tasks, and test cases.

---

## 4.6 Local Development Setup (No Docker Required)

This section documents the minimal steps to run DEV-01A locally with a native MySQL install and no MailHog.

### Prerequisites
- .NET 8 SDK
- MySQL 8 running locally on port 3306
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### Step 1 — Create the database

```sql
-- Run in MySQL Workbench / mysql CLI
CREATE DATABASE IF NOT EXISTS eventmanagement_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### Step 2 — Configure appsettings.Development.json

File: `EventManagement.API/appsettings.Development.json`

Update the `DefaultConnection` string to match your local MySQL credentials:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=eventmanagement_dev;User=root;Password=YOUR_MYSQL_PASSWORD;"
}
```

### Step 3 — Set JWT_SECRET environment variable

```powershell
# PowerShell (current session)
$env:JWT_SECRET = "your-local-dev-secret-minimum-32-characters!!"

# Or persist for the session in a .env approach (never commit this value)
```

> JWT_SECRET must be ≥ 32 characters. The app throws `InvalidOperationException` at startup if missing.

### Step 4 — Run EF migrations

From the `backend/` directory (where `EventManagement.sln` lives):

```powershell
# If projects are directly in backend/ (no /src subfolder)
dotnet ef migrations add S1_AuthCore `
  --project EventManagement.Infrastructure `
  --startup-project EventManagement.API

dotnet ef database update `
  --project EventManagement.Infrastructure `
  --startup-project EventManagement.API

# If projects are under backend/src/
dotnet ef migrations add S1_AuthCore `
  --project src/EventManagement.Infrastructure `
  --startup-project src/EventManagement.API

dotnet ef database update `
  --project src/EventManagement.Infrastructure `
  --startup-project src/EventManagement.API
```

### Step 5 — Run the API

```powershell
# From backend/ directory
dotnet run --project EventManagement.API
# or: dotnet run --project src/EventManagement.API

# Swagger UI: http://localhost:5000/swagger
```

### Email verification without MailHog

The `EmailService` has a built-in dev fallback. When SMTP on `localhost:1025` is unreachable (no MailHog), the service logs the verification URL to the console instead of throwing:

```
[WRN] [DEV] SMTP unavailable — copy this verification URL to complete registration: http://localhost:4200/verify-email?token=...
```

Copy the URL from the API console output and open it in your browser to complete the registration flow.

> **Production note:** In any environment other than `Development`, SMTP failures are re-thrown. Run MailHog or configure a real SMTP relay before deploying.

### Step 6 — Run tests

```powershell
dotnet test
```

Integration tests use EF InMemory — no MySQL connection required to run the test suite.

---

## 7. Implementation Conventions (Non-Negotiable for All Stories)

### 7.1 Backend

| Convention | Rule |
|-----------|------|
| Architecture | Clean Architecture: Domain ← Application ← Infrastructure → API |
| CQRS | MediatR: Commands mutate state, Queries return read models. Never mix. |
| Validation | FluentValidation on all command DTOs. Validation error → HTTP 422 |
| Error responses | RFC 7807 ProblemDetails. Never return raw exception messages. |
| Auth | JWT Bearer. Role claim: `role`. Custom claim: `userId`, `orgId` (where applicable). |
| Password hashing | BCrypt, work factor 12 minimum. |
| Token storage (FE) | `httpOnly` cookie for refresh token. `localStorage` FORBIDDEN for tokens. |
| Timestamps | All `DateTime` stored as UTC in DB. IST conversion in response DTOs only. |
| Logging | Serilog structured. Log level: Warning+ in prod. PII (email, name, phone) MUST NOT appear in log messages — use `UserId` instead. |
| DB migrations | EF Core code-first. One migration per story. Never edit existing migrations. |
| Tests | xUnit. Integration tests use `WebApplicationFactory` + test DB (separate schema). |

### 7.2 Frontend

| Convention | Rule |
|-----------|------|
| Architecture | Feature modules (lazy-loaded). `core/` singleton services. `shared/` dumb components. |
| State | Angular services with `BehaviorSubject` for now (NgRx = V1.1). |
| HTTP | All API calls through service layer. Never call `HttpClient` from components. |
| Auth tokens | Access token: in-memory (service property). Refresh token: `httpOnly` cookie (set by backend). |
| Routes | Route guards on all private routes. `returnUrl` preserved on redirect to login. |
| Error handling | HTTP interceptor catches 401 → redirect. 422 → field-level error. 5xx → error toast. |
| Currency | Always `₹` prefix. Indian numbering (lakhs/crores). Use `CurrencyDisplayComponent`. |
| Dates | Always suffix "IST". Use `DateTimeDisplayComponent`. Never use browser locale for dates. |
| Forms | Reactive Forms only. No template-driven forms. |
| Tests | Jasmine/Karma (unit). Playwright (E2E, S9). |

---

## 8. Next Steps

1. **Read `DEV-01-story-auth-core.md`** — Story 1 full spec with ACs and task breakdown
2. **Run `bmad-dev-story` skill** pointing at `DEV-01-story-auth-core.md` to execute Story 1
3. **After Story 1 merges:** generate `DEV-02-story-auth-completion.md` (US-E1-005/006/007 + infra stories)
4. **Before S5 starts:** ensure PRE-06 (architect locking pattern sign-off) is resolved

---

*Amelia — Senior Software Engineer. Story files use `file:` paths and AC IDs. No fluff, all precision.*
