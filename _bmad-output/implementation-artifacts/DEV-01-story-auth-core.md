# DEV-01 — Story: Authentication Core (Sprint 1)

**Story ID:** US-E1-001 + US-E1-002 + US-E1-003 + US-E1-004
**Sprint:** S1
**Points:** 14 SP
**Owner:** BE + FE
**Dependencies:** None (zero-dep story group)
**Status:** Ready for implementation
**Phased Execution:** DEV-01A (Backend Foundation) → DEV-01B (Frontend UI Integration)

> This is the foundational story. No other story can start until this merges.
> Phase order is mandatory: DEV-01A must be merged and API verified before DEV-01B begins.

---

## Context

V1 booking model: instant confirmation only. No payment. JWT auth. MySQL 8. ASP.NET Core Web API. Angular frontend. SMTP via MailHog (dev). BCrypt passwords. Refresh token via `httpOnly` cookie.

Two registrant roles in this story: `Attendee` and `Organizer`. The Organizer registration creates a `PendingApproval` organizer record in addition to the `User` row — organizer is NOT active until admin approves (Sprint 2). This story establishes the user + token infrastructure only.

### Auth State Model — Organizer Dual-Status Design

Critical: `User.Status` and `Organization.Status` are independent lifecycles.

| Concept | Controls | Values |
|---------|---------|--------|
| `User.Status` | Identity lifecycle — can the user authenticate at all? | `PendingVerification` → `Active` → `Suspended` / `Deactivated` |
| `Organization.Status` | Business access — can the organizer perform organizer actions? | `PendingApproval` → `Active` → `Suspended` / `Rejected` |

**Rule:** An Organizer user CAN log in once their email is verified (`User.Status=Active`). They receive a JWT. However, all organizer-only API endpoints and the organizer dashboard are blocked by `OrganizerGuard` until `Organization.Status=Active`.

**Implementation:** `OrganizerGuard` (middleware + Angular route guard) must:
1. Verify JWT is valid (`User.Status=Active` implied by token issuance)
2. Load `Organization` record for the JWT's `userId`
3. If `Organization.Status ≠ Active` → HTTP 403 `{ detail: "Organization pending admin approval" }` (BE) / redirect to `/organizer/pending-approval` status page (FE)

This design means the JWT claim `role=Organizer` alone is insufficient for organizer access — the guard always checks live `Organization.Status`. Do not cache this value in the JWT.

**S1 Migration scope:** `Organizations` table (minimal schema) is locked into the `S1_AuthCore` migration alongside `Users`. Required because organizer registration in US-E1-002 writes to both tables atomically.

---

## Acceptance Criteria

### AC-E1-001-01 — Attendee Registration (Happy Path)
```gherkin
Given I am on /register/attendee
When I submit: FullName="Raj Kumar", Email="raj@example.com",
               Phone="9876543210", City="Chennai",
               Password="Str0ng@Pass!", ConfirmPassword="Str0ng@Pass!",
               TermsAccepted=true
Then HTTP 201 is returned with { userId, email, status: "PendingVerification" }
And a User row exists in DB with Role="Attendee", Status="PendingVerification"
And an EmailVerificationToken row exists, ExpiresAt = now + 24h
And a ConsentLedger row exists: ConsentType="Registration", TncVersion set, IP logged
And a verification email is queued via SMTP
```

### AC-E1-001-02 — Duplicate Email Rejected
```gherkin
Given a User with Email="raj@example.com" already exists
When POST /api/auth/register/attendee with same email
Then HTTP 409 with ProblemDetails: title="Conflict", detail="Email already registered"
And no new user row is created
```

### AC-E1-001-03 — Weak Password Rejected
```gherkin
Given I submit Password="abc123"
Then HTTP 422 with FluentValidation errors listing all unmet password rules
And no user row is created
Rules: min 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char
```

### AC-E1-001-04 — Missing Required Fields
```gherkin
Given I submit registration with Email="" (or any required field empty/null)
Then HTTP 422 with field-level validation errors
```

### AC-E1-002-01 — Organizer Registration (Happy Path)
```gherkin
Given I am on /register/organizer
When I submit all required organizer fields including OrganizationName,
     ContactPerson, Email, Phone, Category, Password
Then HTTP 201 with { userId, orgId, email, status: "PendingApproval" }
And a User row exists: Role="Organizer", Status="Active" (user is active; org is pending)
And an Organization row exists: Status="PendingApproval"
And an EmailVerificationToken row exists
And verification email queued
```

### AC-E1-002-02 — Organizer Cannot Act Until Approved
```gherkin
Given an Organizer user is Active (email verified) but Organization.Status="PendingApproval"
When the organizer attempts GET /api/organizer/dashboard (or any /api/organizer/** endpoint)
Then HTTP 403 with ProblemDetails: detail="Organization pending admin approval"
And the JWT is NOT invalidated (user remains logged in)
And FE OrganizerGuard redirects to /organizer/pending-approval (not /login)
```

### AC-E1-002-03 — Organizer Login Permitted Before Org Approval
```gherkin
Given an Organizer user is Active (email verified) but Organization.Status="PendingApproval"
When POST /api/auth/login with correct credentials
Then HTTP 200 with accessToken (role=Organizer)
And orgStatus="PendingApproval" is included in the login response body
And FE auth service reads orgStatus and routes to /organizer/pending-approval
```

### AC-E1-003-01 — Successful Email Verification
```gherkin
Given a User with Status="PendingVerification"
And a valid, unexpired EmailVerificationToken T
When GET /api/auth/verify-email?token=T
Then HTTP 200
And User.Status → "Active"
And EmailVerificationToken.UsedAt = now
And I can now log in
```

### AC-E1-003-02 — Expired Verification Token
```gherkin
Given an EmailVerificationToken with ExpiresAt < now
When GET /api/auth/verify-email?token=expiredToken
Then HTTP 400 with detail="Verification link has expired. Please request a new one."
And User.Status remains "PendingVerification"
```

### AC-E1-003-03 — Already Used Token
```gherkin
Given an EmailVerificationToken with UsedAt IS NOT NULL
When GET /api/auth/verify-email?token=usedToken
Then HTTP 400 with detail="This verification link has already been used."
```

### AC-E1-003-04 — Resend Verification Email
```gherkin
Given a User with Status="PendingVerification" and email="raj@example.com"
When POST /api/auth/resend-verification { email: "raj@example.com" }
Then HTTP 200 with { message: "Verification email resent if account exists" }
And a new EmailVerificationToken row is created (ExpiresAt = now + 24h)
And any prior unexpired tokens for the same user are invalidated (UsedAt set)
And a new verification email is dispatched

Scenario: Email does not exist (no enumeration)
Given no User exists with email="nobody@example.com"
When POST /api/auth/resend-verification { email: "nobody@example.com" }
Then HTTP 200 with the same generic message (no indication whether email exists)
And no email is sent
And no new token row is created

Scenario: Already verified account
Given a User with Status="Active" and email="active@example.com"
When POST /api/auth/resend-verification { email: "active@example.com" }
Then HTTP 200 with { message: "Verification email resent if account exists" }
And no email is sent (silently ignored — user is already verified)
And no new token row is created
```

### AC-E1-004-01 — Successful Login
```gherkin
Given a User with Status="Active", correct credentials
When POST /api/auth/login { email, password }
Then HTTP 200 with { accessToken (JWT, 30 min), userId, email, role, orgStatus? }
And a RefreshToken row created in DB (hashed)
And Set-Cookie: refreshToken=<value>; HttpOnly; Secure; Path=/api/auth/refresh
     (SameSite=Lax in Development; SameSite=Strict in Production — env-aware)
And JWT claims include: sub=userId, role=role, email=email, exp=now+30min
Note: orgStatus is only present in response when role=Organizer (null otherwise)
```

### AC-E1-004-02 — Unverified Account Login Blocked
```gherkin
Given User.Status="PendingVerification"
When POST /api/auth/login with correct credentials
Then HTTP 401 with detail="Please verify your email before logging in."
```

### AC-E1-004-03 — Wrong Password
```gherkin
Given User.Status="Active"
When POST /api/auth/login with wrong password
Then HTTP 401 with detail="Invalid email or password."
And a LoginAttempt row logged (Succeeded=false)
```

### AC-E1-004-04 — Token Refresh
```gherkin
Given a valid refresh token in httpOnly cookie
When POST /api/auth/refresh (cookie auto-sent by browser)
Then HTTP 200 with new accessToken
And old refresh token revoked (RevokedAt set)
And new RefreshToken row created
```

### AC-E1-004-05 — Logout
```gherkin
When POST /api/auth/logout (with valid access token)
Then HTTP 204
And RefreshToken.RevokedAt = now for all user's active refresh tokens
And Set-Cookie: refreshToken=; Max-Age=0 (cookie cleared)
```

---

## DEV-01A — Backend Foundation

> Complete DEV-01A, verify API with Swagger + MailHog, run all BE tests before starting DEV-01B.

#### BE-T01 — Domain entities
**File:** `backend/src/EventManagement.Domain/Entities/User.cs`
- Properties: Id, Email, PasswordHash, FullName, Phone, City, Role (enum), Status (enum), CreatedAt, UpdatedAt
- Enums in `EventManagement.Domain/Enums/UserRole.cs` and `UserStatus.cs`

**File:** `backend/src/EventManagement.Domain/Entities/EmailVerificationToken.cs`
- Properties: Id, UserId, Token, ExpiresAt, UsedAt, CreatedAt

**File:** `backend/src/EventManagement.Domain/Entities/RefreshToken.cs`
- Properties: Id, UserId, TokenHash, ExpiresAt, RevokedAt, CreatedAt

**File:** `backend/src/EventManagement.Domain/Entities/ConsentLedger.cs`
- Properties: Id, UserId (nullable), SessionId, ConsentType (enum), TncVersion, IpAddress, UserAgent, ConsentGivenAt

**File:** `backend/src/EventManagement.Domain/Entities/LoginAttempt.cs`
- Properties: Id, Email, AttemptedAt, Succeeded, IpAddress

**File:** `backend/src/EventManagement.Domain/Entities/Organization.cs`
*(minimal for this story — full org fields in S2)*
- Properties: Id, UserId (FK), OrganizationName, ContactPerson, Category, Status (enum: PendingApproval, Active, Suspended, Rejected), CreatedAt
- **Locked to S1 migration** — required for atomic organizer registration (US-E1-002 writes User + Organization in one transaction)

#### BE-T02 — EF Core DbContext + Migration
**File:** `backend/src/EventManagement.Infrastructure/Persistence/AppDbContext.cs`
- DbSets: Users, EmailVerificationTokens, RefreshTokens, ConsentLedger, LoginAttempts, Organizations
- Configure: unique index on Users.Email; index on LoginAttempts(Email, AttemptedAt); index on RefreshTokens(UserId)

**Command:** `dotnet ef migrations add S1_AuthCore --project EventManagement.Infrastructure --startup-project EventManagement.API`

#### BE-T03 — Application layer commands + handlers
**Files in** `backend/src/EventManagement.Application/Features/Auth/`:

```
RegisterAttendee/
  RegisterAttendeeCommand.cs     — FullName, Email, Phone, City, Password, TncVersion, IpAddress, UserAgent
  RegisterAttendeeCommandHandler.cs
  RegisterAttendeeCommandValidator.cs  — FluentValidation: email format, password rules, phone 10 digits

RegisterOrganizer/
  RegisterOrganizerCommand.cs
  RegisterOrganizerCommandHandler.cs
  RegisterOrganizerCommandValidator.cs

VerifyEmail/
  VerifyEmailCommand.cs          — Token (string)
  VerifyEmailCommandHandler.cs

ResendVerificationEmail/
  ResendVerificationEmailCommand.cs    — Email (string)
  ResendVerificationEmailCommandHandler.cs
  └─ if User not found OR User.Status=Active: return success silently (no enumeration)
  └─ if User.Status=PendingVerification: invalidate prior tokens, issue new token, send email

Login/
  LoginCommand.cs                — Email, Password, IpAddress, UserAgent
  LoginCommandHandler.cs         — check status, verify BCrypt, issue JWT + refresh token
                                    if role=Organizer: load Organization.Status, include in response
  LoginCommandValidator.cs

RefreshToken/
  RefreshTokenCommand.cs         — (reads from httpOnly cookie via controller)
  RefreshTokenCommandHandler.cs

Logout/
  LogoutCommand.cs               — UserId
  LogoutCommandHandler.cs
```

#### BE-T04 — Infrastructure services
**File:** `backend/src/EventManagement.Infrastructure/Services/PasswordHasher.cs`
- BCrypt.Net, work factor 12

**File:** `backend/src/EventManagement.Infrastructure/Services/JwtTokenService.cs`
- Issue access token: 30-min expiry, claims: sub, role, email, userId
- Sign with HS256, secret from env var `JWT_SECRET` (≥256-bit)

**File:** `backend/src/EventManagement.Infrastructure/Services/EmailService.cs`
- Interface: `IEmailService` in Application layer
- Impl: MailKit SMTP client. Config from `IOptions<SmtpSettings>`
- Method: `SendVerificationEmailAsync(email, fullName, verificationUrl)`

**File:** `backend/src/EventManagement.Infrastructure/Services/ConsentLedgerService.cs`
- Interface: `IConsentLedgerService` in Application layer
- Method: `RecordConsentAsync(userId, consentType, tncVersion, ipAddress, userAgent)`

#### BE-T05 — API controllers
**File:** `backend/src/EventManagement.API/Controllers/AuthController.cs`

```csharp
[Route("api/auth")]
POST /register/attendee    → RegisterAttendeeCommand
POST /register/organizer   → RegisterOrganizerCommand
GET  /verify-email         → VerifyEmailCommand (token from query string)
POST /login                → LoginCommand → sets httpOnly cookie + returns access token
POST /refresh              → RefreshTokenCommand (reads cookie)
POST /logout               → LogoutCommand [Authorize]
```

Security: extract IpAddress from `X-Forwarded-For` (fallback: `HttpContext.Connection.RemoteIpAddress`). Never trust client-supplied IP for rate limiting.

#### BE-T06 — Middleware and configuration
**File:** `backend/src/EventManagement.API/Middleware/CorrelationIdMiddleware.cs`
- Reads `X-Correlation-Id` header or generates new GUID. Adds to response + Serilog context.

**File:** `backend/src/EventManagement.API/Middleware/OrganizerGuard.cs` (ASP.NET Core policy / authorization handler)
- Applied as `[Authorize(Policy = "ActiveOrganizer")]` on all `/api/organizer/**` controllers
- Policy requirement: JWT valid (User.Status=Active implied) **AND** `Organization.Status=Active` (loaded from DB by UserId claim)
- On failure: HTTP 403 `{ detail: "Organization pending admin approval" }`
- **Do not cache Organization.Status in the JWT.** Always read live from DB (single indexed lookup on `Organizations.UserId`).
- Register in `Program.cs`: `services.AddAuthorization(o => o.AddPolicy("ActiveOrganizer", p => p.AddRequirements(new ActiveOrganizerRequirement())))`

**File:** `backend/src/EventManagement.API/Configuration/CookieSettings.cs`
- Properties: `SameSiteMode SameSite` (bound from config)
- `appsettings.Development.json`: `"Cookie": { "SameSite": "Lax" }`
- `appsettings.Production.json`: `"Cookie": { "SameSite": "Strict" }`
- Used in `AuthController` when setting the refresh token cookie:
```csharp
var sameSite = _cookieSettings.SameSite; // Lax in dev, Strict in prod
new CookieOptions { HttpOnly = true, Secure = true, SameSite = sameSite, Path = "/api/auth/refresh" }
```

**File:** `backend/src/EventManagement.API/Configuration/JwtSettings.cs`
- Bound from `appsettings.json:Jwt` section. Secret loaded from env var only — not from appsettings.

**File:** `backend/src/EventManagement.API/Configuration/SmtpSettings.cs`
- Host, Port, Username, Password (all nullable for MailHog dev), FromEmail, FromName

#### BE-T07 — Tests
**File:** `backend/tests/EventManagement.UnitTests/Features/Auth/RegisterAttendeeCommandHandlerTests.cs`
- Happy path creates user + token + consent row
- Duplicate email throws domain exception
- Weak password validator rejects

**File:** `backend/tests/EventManagement.UnitTests/Features/Auth/LoginCommandHandlerTests.cs`
- Happy path returns token pair
- Unverified account rejected
- Wrong password rejected + LoginAttempt row logged

**File:** `backend/tests/EventManagement.IntegrationTests/Auth/AuthEndpointTests.cs`
- POST /register/attendee → 201
- POST /register/attendee (duplicate) → 409
- POST /register/organizer → 201, Organization row Status=PendingApproval
- GET /verify-email?token=valid → 200 + user active
- GET /verify-email?token=expired → 400
- GET /verify-email?token=used → 400
- POST /resend-verification (pending user) → 200 + new token in DB + prior tokens invalidated
- POST /resend-verification (non-existent email) → 200 (no enumeration)
- POST /resend-verification (already active user) → 200 (no email sent)
- POST /login (active attendee) → 200 + cookie set (no orgStatus in response)
- POST /login (active organizer, org pending) → 200 + orgStatus="PendingApproval" in response
- POST /login (unverified) → 401
- POST /refresh → 200 + new access token
- GET /api/organizer/dashboard (organizer, org pending) → 403 (OrganizerGuard test)

---

## DEV-01B — Frontend UI Integration

> Start only after DEV-01A is merged, all BE integration tests pass, and API is reachable at localhost.

#### FE-T01 — Core module setup
**File:** `frontend/src/app/core/services/auth.service.ts`
- `currentUser$: BehaviorSubject<User | null>`
- Methods: `login()`, `logout()`, `refreshToken()`, `isAuthenticated()`, `hasRole(role)`
- Access token stored in memory (service property only, NOT localStorage)

**File:** `frontend/src/app/core/interceptors/jwt.interceptor.ts`
- Attaches `Authorization: Bearer <accessToken>` to all API requests
- On 401: attempts token refresh once, then redirects to `/login?returnUrl=...`

**File:** `frontend/src/app/core/guards/organizer.guard.ts`
- `canActivate()`: checks `authService.isAuthenticated()` AND `authService.currentUser$.orgStatus === 'Active'`
- If orgStatus ≠ Active: redirect to `/organizer/pending-approval` (NOT `/login`)
- `orgStatus` is stored in `AuthService.currentUser$` from login response — refreshed on token refresh

**File:** `frontend/src/app/organizer/pending-approval/pending-approval.component.ts`
- Static page: "Your organisation is pending admin approval. You’ll receive an email once approved."
- Accessible to logged-in organizers with any org status (not guarded)
- Include [Logout] link

#### FE-T02 — Attendee registration form
**File:** `frontend/src/app/public/register-attendee/register-attendee.component.ts`
- Reactive Form: fullName*, email*, phone* (+91 prefix fixed), city*, password*, confirmPassword*, termsAccepted*
- Password strength meter (5-level: Very Weak / Weak / Fair / Strong / Very Strong)
- Individual requirement checklist: 8+ chars ✓, uppercase ✓, lowercase ✓, digit ✓, special ✓
- Inline validation on blur; submit disabled until all valid
- On submit: POST to `/api/auth/register/attendee`; success → navigate to `/register/success`

**File:** `frontend/src/app/public/register-attendee/register-attendee.component.html`
- Angular Material form fields; no custom styling that overrides Material accessibility

#### FE-T03 — Organizer registration form (4-step wizard)
**File:** `frontend/src/app/public/register-organizer/register-organizer.component.ts`
- Step 1: orgName*, contactPerson*, email*, phone*, category*, password*, confirmPassword*
- Step 2: address*, city*, state*, pincode*, website?, socialLinks?
- Step 3: document uploads (PAN, GSTIN, ID Proof, Business Reg — all optional)
- Step 4: review summary + submit
- `MatStepper` (linear mode)

#### FE-T04 — Login form
**File:** `frontend/src/app/public/login/login.component.ts`
- Reactive Form: email*, password*
- On 401 (unverified): show "Please verify your email. [Resend verification link]" message
- On 401 (wrong password): "Invalid email or password."
- On success: read `returnUrl` from query params → navigate there, else default by role:
  - Attendee → `/my/dashboard`
  - Organizer → `/organizer/dashboard`
  - SuperAdmin → `/admin/dashboard`
  - CheckinStaff → `/checkin/events`

#### FE-T05 — Email verification page
**File:** `frontend/src/app/public/verify-email/verify-email.component.ts`
- On init: read `token` from query params → call `GET /api/auth/verify-email?token=`
- Success state: "Email verified! [Login now →]"
- Error state: "This link has expired or already been used. [Request new verification link]" → button calls `POST /api/auth/resend-verification { email }` (prompt user for email if not in query params)
- Resend success state: "New verification link sent! Check your inbox."
- Loading state: spinner (no content flash)
- Rate-limit awareness: if resend returns 429, show "Please wait before requesting another link."

#### FE-T06 — Registration success page
**File:** `frontend/src/app/public/register-success/register-success.component.ts`
- Static message: "Check your email! We sent a verification link to [email]. It expires in 24 hours."
- No auto-redirect (user controls navigation)

#### FE-T07 — FE unit tests
**File:** `frontend/src/app/public/register-attendee/register-attendee.component.spec.ts`
- Form invalid when required fields empty
- Password strength meter updates on input
- Terms checkbox required
- HTTP call made on valid submit

**File:** `frontend/src/app/core/services/auth.service.spec.ts`
- `login()` stores access token in memory
- `logout()` clears token + calls POST /logout
- `isAuthenticated()` returns false when no token

---

## Definition of Done

- [ ] All AC scenarios pass (unit + integration tests)
- [ ] `dotnet test` passes with ≥60% coverage on Domain + Application layers
- [ ] `ng test --watch=false` passes with zero failures
- [ ] EF Core migration applies cleanly on fresh DB (`S1_AuthCore` — includes Users + Organizations)
- [ ] JWT access token verified by Swagger (Authorize button works)
- [ ] Refresh token set as `httpOnly` cookie in browser dev tools (SameSite=Lax in dev, SameSite=Strict in prod)
- [ ] Verification email visible in MailHog (`http://localhost:8025`)
- [ ] Resend-verification email visible in MailHog; prior token invalidated in DB
- [ ] OrganizerGuard returns 403 for organizer with org Status=PendingApproval (verified by integration test)
- [ ] Organizer login response includes `orgStatus` field when role=Organizer
- [ ] ProblemDetails returned for all error paths (no raw exception leakage)
- [ ] No PII (email, name, phone) in Serilog log output — only `UserId` references
- [ ] Swagger UI documents all 7 endpoints in `/api/auth`
- [ ] CORS allows `localhost:4200` in dev (not in prod config)
- [ ] `dotnet ef migrations list` shows exactly 1 migration: `S1_AuthCore`

---

## Security Checklist (OWASP Top 10 for this story)

| Risk | Mitigation in this story |
|------|--------------------------|
| **A02 — Cryptographic Failures** | BCrypt work factor 12. JWT secret ≥256-bit from env var. Refresh token: hash stored (SHA-256), raw value only in cookie. |
| **A03 — Injection** | EF Core parameterized queries exclusively. FluentValidation before handler. |
| **A05 — Security Misconfiguration** | CORS restricted to dev origin only. Cookie: HttpOnly, Secure, SameSite=Lax (dev) / SameSite=Strict (prod) — controlled via `CookieSettings` config binding, not hardcoded. |
| **A07 — Identification & Auth Failures** | Account lockout (S1-E1-006 companion). No user enumeration: same 401 for "wrong password" and "no such user". Resend-verification returns 200 regardless of email existence (AC-E1-003-04). |
| **A09 — Logging Failures** | Correlation ID on every request. LoginAttempts logged. But PII redacted from structured logs. |

---

## API Contract (OpenAPI Summary)

```
POST /api/auth/register/attendee
  Request:  RegisterAttendeeDto { fullName, email, phone, city, password, confirmPassword, tncVersion }
  Response 201: { userId, email, status }
  Response 409: ProblemDetails (duplicate email)
  Response 422: ValidationProblemDetails

POST /api/auth/register/organizer
  Request:  RegisterOrganizerDto { orgName, contactPerson, email, phone, category, password, confirmPassword, address, city, state, pincode, website? }
  Response 201: { userId, orgId, email, orgStatus: "PendingApproval" }
  Response 409 / 422: same as above

GET  /api/auth/verify-email?token={token}
  Response 200: { message: "Email verified" }
  Response 400: ProblemDetails

POST /api/auth/resend-verification
  Request:  { email: string }
  Response 200: { message: "Verification email resent if account exists" }
  (Always 200 regardless of whether email exists — anti-enumeration)
  Response 422: ValidationProblemDetails (invalid email format)

POST /api/auth/login
  Request:  LoginDto { email, password }
  Response 200: { accessToken, expiresIn: 1800, userId, email, role, orgStatus?: string }
            + Set-Cookie: refreshToken (httpOnly; SameSite=Lax/Strict per env)
  Response 401: ProblemDetails

POST /api/auth/refresh
  Request:  (no body — reads httpOnly cookie)
  Response 200: { accessToken, expiresIn: 1800 }
  Response 401: ProblemDetails (no/invalid/expired cookie)

POST /api/auth/logout  [Authorize]
  Request:  (no body)
  Response 204
  Response 401: if not authenticated
```

---

## Estimated Effort

| Layer | Estimated Hours |
|-------|----------------|
| BE Domain entities + migrations | 2h |
| BE Commands + validators + handlers | 6h |
| BE Controllers + middleware | 3h |
| BE Infrastructure (JWT, BCrypt, MailKit, Consent) | 4h |
| BE Tests (unit + integration) | 5h |
| FE Core module (auth service, interceptor, guard) | 4h |
| FE Registration forms (attendee + organizer) | 5h |
| FE Login + verify-email | 3h |
| FE Tests | 3h |
| **Total** | **~35h** |

*Solo developer: ~3–4 days. Team (1 BE + 1 FE): ~2 days.*

---

*Next story file: DEV-02 (US-E1-005/006/007 + US-E7-001/008 + US-E10-003/005/008) — Sprint 1 completion.*
