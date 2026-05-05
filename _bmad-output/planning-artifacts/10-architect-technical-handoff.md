# 10 — Architect Technical Handoff

**To:** Winston (System Architect)
**From:** Mary (Business Analyst)
**Date:** 2026-05-05
**Status:** Ready for architecture phase

---

## 1. Brief

Design the technical solution architecture for the **Smart Event Management & Ticket Booking Platform — V1**, an India-only, INR-only, English-only web platform built on free/open-source technology with a fixed tech stack (Angular + ASP.NET Core Web API + MySQL + JWT + SMTP + open-source QR libraries).

V1 has **no payment gateway**, **no SMS / push**, **no native mobile app**, and **no white-labeling** — but must be **payment-ready, multi-channel-ready, and i18n-ready** so Phase 2 enhancements drop in without redesign.

---

## 2. Locked Inputs

| Area | Decision |
|------|----------|
| Frontend | Angular (latest LTS) |
| Backend | ASP.NET Core Web API (latest LTS) |
| Database | MySQL 8.x |
| Auth | JWT (access + refresh) |
| Storage | Local server filesystem behind `IStorageProvider` |
| Email | SMTP (provider TBD — Brevo/SendGrid free tier candidates) |
| QR | Free open-source libraries (e.g., **QRCoder** for .NET; **ngx-scanner** / **html5-qrcode** for Angular) |
| Background Jobs | Hangfire or Quartz.NET (free) |
| CI/CD | GitHub Actions |
| Hosting | Low-cost VPS, Docker-based |
| Environments | Local Dev + Staging + Production |
| HTTPS | Let's Encrypt (free) |

---

## 3. Architectural Style Recommendations

- **Modular Monolith** for V1 (single deployable Web API + single Angular app), with **clean module boundaries** so future extraction to microservices (e.g., Notification, Reporting) is feasible.
- **Layered architecture per module:** Controller → Service → Repository → DB.
- **CQRS-light:** read models for dashboards/reports may bypass the full service layer for performance; writes always go through services.
- **Event-driven internal messaging** (in-process MediatR or similar) to decouple booking → notification, organizer-state-change → audit, etc.
- **Stateless API** (JWT-based); no in-memory session state. Enables future horizontal scaling.

---

## 4. Critical Architectural Concerns to Resolve

1. **Atomic Inventory Decrement** — Choose between MySQL `SELECT … FOR UPDATE` row lock vs. optimistic concurrency with retry. Recommendation: pessimistic row-lock for V1 simplicity & guarantee.
2. **QR Token Strategy** — HMAC-SHA256 token format, secret rotation plan (key-id in token payload), rejection on signature failure.
3. **Background Job Engine Choice** — Hangfire (richer dashboard, MySQL-backed) vs. Quartz.NET (lighter). Recommendation: **Hangfire** for built-in admin UI and persistence in MySQL.
4. **File Storage Abstraction** — Define `IStorageProvider` interface from day 1; implement `LocalFileStorageProvider`. Phase 2: `S3StorageProvider`.
5. **Notification Channel Abstraction** — Define `INotificationChannel` with `SmtpEmailChannel` impl. Phase 2: `SmsChannel`, `PushChannel` plug in.
6. **Multi-Tenant-ish Data Scoping** — All organizer-scoped queries must include organizer_id filter at repository level (defense in depth).
7. **Atomic Transactional Boundary for Booking** — Inventory + booking + tickets + QR token + audit log all in one DB transaction; email queued post-commit.
8. **Audit Log Pattern** — Single generic `audit_log` table; insert from any service via shared `IAuditWriter`.
9. **Time Handling** — Store all timestamps in UTC; display IST in UI; backend uses `DateTimeOffset`.
10. **Rate Limiting** — Use ASP.NET Core 7+ built-in rate limiter middleware; per-endpoint policies.

---

## 5. Suggested High-Level Component View

```
[ Angular SPA ]
    ├── Public Site Module
    ├── Attendee Module
    ├── Organizer Module
    ├── Admin Module
    └── Check-in Scanner Module
              │
              ▼ HTTPS (JWT)
[ ASP.NET Core Web API ]
    ├── Controllers (thin)
    ├── Application Services (one per business module)
    ├── Domain Models + Business Rules
    ├── Repositories (EF Core)
    ├── Cross-cutting: Auth, Authz, Logging, Audit, Rate-Limit, i18n, Health
    ├── Notification Service (channel abstraction)
    ├── Storage Service (provider abstraction)
    ├── QR Token Service (HMAC sign/verify)
    └── Background Jobs (Hangfire)
              │
              ▼
[ MySQL 8.x ]   [ Local Filesystem ]   [ SMTP Provider ]
```

---

## 6. Key Patterns & Libraries Suggested

| Concern | Library / Pattern |
|---------|-------------------|
| ORM | **EF Core** (Pomelo MySQL provider) |
| Validation | **FluentValidation** |
| Mediator / in-process events | **MediatR** |
| Background Jobs | **Hangfire** (MySQL storage) |
| QR Generation | **QRCoder** |
| QR Scanning (browser) | **html5-qrcode** or **ngx-scanner** |
| PDF e-ticket | **QuestPDF** (free for SMB) |
| HTML Email Templates | **Razor templates** + **MailKit** for SMTP |
| Logging | **Serilog** (JSON sink) |
| OpenAPI / Swagger | Built-in **Swashbuckle** |
| Migrations | EF Core Migrations |
| Testing | xUnit + FluentAssertions; Cypress for E2E |

---

## 7. Database Hints

(Detailed entity model in Document 11.)

- Single MySQL schema; use snake_case table names.
- All PKs are `BIGINT AUTO_INCREMENT` + a public `UUID` column for external references where appropriate (e.g., events, bookings).
- Booking reference (`BK-YYYY-NNNNNN`) generated server-side via a dedicated sequence per year.
- All money columns: `DECIMAL(12,2)` INR.
- All timestamps: `DATETIME(6)` UTC.
- `audit_log` table — append-only, `INSERT` permission only (enforce via app-level `IAuditWriter`).
- Indices on hot paths: `events(status, start_time)`, `tickets(qr_token_hash)` (unique), `bookings(attendee_id, status)`, `coupons(code)` (unique).

---

## 8. Security Architecture

- **Authentication:** JWT (HS256 with strong secret, or RS256 if you anticipate distributed verification later).
- **Authorization:** Role-based (`SuperAdmin`, `Organizer`, `Attendee`, `CheckinStaff`) + ownership checks via policy handlers.
- **HTTPS:** Enforced via `UseHsts` + redirect; Let's Encrypt cert via certbot/sidecar.
- **Headers:** CSP (script-src self), X-Frame-Options DENY, X-Content-Type-Options nosniff, Referrer-Policy strict-origin-when-cross-origin.
- **Rate Limiting:** Endpoint-specific via ASP.NET Core rate limiter.
- **OWASP Baseline:** Parameterized queries (EF Core), Angular auto-escaping, anti-forgery for forms, file upload validation, secret management via env vars.
- **QR HMAC Secret:** Stored in env var; rotation plan documented; key-id in token enables rotation.
- **Audit Log:** Insert-only with DB constraints / triggers preventing UPDATE & DELETE.

---

## 9. Performance & Scalability Architecture

- Stateless API → can scale horizontally behind a reverse proxy (nginx) when traffic justifies.
- MySQL primary; read replicas Phase 2.
- DB connection pooling (default in EF Core).
- Pagination on all list endpoints (default 20, max 100).
- No Redis in V1; design cache-friendly query patterns so adding Redis later is non-disruptive.
- CDN for static assets in Phase 2 (Cloudflare free tier candidate).

---

## 10. Background Jobs (Hangfire) — V1 Catalog

| Job | Schedule | Purpose |
|-----|----------|---------|
| `EventAutoCompleteJob` | Every 15 min | Transition Published → Completed when end_time passed |
| `Reminder24hJob` | Hourly | Send reminders for events 24–25h ahead |
| `Reminder2hJob` | Every 15 min | Send reminders for events 2h–2h15m ahead |
| `PostEventFeedbackJob` | Hourly | Send feedback request 24h after event end |
| `SmtpRetryJob` | Continuous worker | Process failed-email retry queue (3 attempts) |
| `TicketExpiryJob` | Hourly | Mark Issued tickets Expired after event end + 2h |
| `BackupJob` | Daily 03:00 IST | Trigger DB dump + rotate retention |

---

## 11. Deployment Topology (V1)

```
┌────────────────────────────────────────────────────────────────┐
│                     Single VPS (Production)                    │
│   ┌─────────────────────────────────────────────────────────┐  │
│   │  nginx (reverse proxy + TLS termination)                │  │
│   └──────────┬─────────────────────┬────────────────────────┘  │
│              ▼                     ▼                            │
│   ┌────────────────────┐  ┌────────────────────┐               │
│   │ Angular static SPA │  │ ASP.NET Core API   │               │
│   │ (served by nginx)  │  │ (Docker container) │               │
│   └────────────────────┘  └─────────┬──────────┘               │
│                                     │                           │
│                       ┌─────────────┴────────────┐              │
│                       ▼                          ▼              │
│              ┌──────────────────┐      ┌──────────────────┐    │
│              │   MySQL 8.x      │      │ Hangfire Worker  │    │
│              │   (Docker)       │      │ (same image as   │    │
│              └──────────────────┘      │   API or sep.)   │    │
│                                        └──────────────────┘    │
│              Local /var/app/uploads (volume-mounted)           │
└────────────────────────────────────────────────────────────────┘
       │                                        │
       ▼                                        ▼
   Daily DB backup → off-host storage      External SMTP provider
```

---

## 12. CI/CD Pipeline (GitHub Actions)

1. **On PR:** lint, unit tests, build both apps, run security scan (e.g., `dotnet list package --vulnerable`).
2. **On merge to `develop`:** build images → push to registry → deploy to Staging.
3. **On tag/release:** deploy to Production with manual approval gate.
4. Migrations applied automatically with rollback safety (transactional migrations where possible).

---

## 13. Phase 2 Forward-Compatibility Hooks

| Phase 2 Feature | V1 Hook |
|------------------|---------|
| Payment gateway | `payment_status` column on booking; `Reserved/Pay-Later` mode; price metadata always captured |
| SMS / Push | `INotificationChannel` abstraction in V1 |
| Cloud storage | `IStorageProvider` abstraction in V1 |
| Multi-language | i18n message keys throughout V1 |
| Recurring events | Event entity allows nullable `recurrence_rule` column reserved for V2 |
| Organizer monetization | Reserved nullable `tier`, `commission_rate` columns on organizer table |
| Native app | All core flows behind REST APIs; Swagger contract published |
| RBAC sub-roles | `roles` and `user_roles` tables structured for many-to-many even though V1 uses single role |

---

## 14. Open Questions for Architect to Decide

1. JWT signing: HS256 vs RS256?
2. Hangfire vs Quartz.NET (Mary recommends Hangfire — confirm).
3. EF Core code-first vs database-first migrations strategy.
4. Single API project vs modular project per bounded context.
5. Anti-forgery strategy with JWT (cookie + header double-submit, or pure Authorization header + SameSite policy)?
6. SMTP provider final choice (Brevo / SendGrid / AWS SES free tier).
7. Backup destination (off-host volume / S3-compatible object store / Borg/Restic to remote).

---

## 15. Deliverables Expected from Architect

- Technical Architecture Document (TAD)
- Component & Container diagrams (C4)
- Detailed sequence diagrams (booking, check-in, organizer onboarding)
- Database physical model (DDL)
- API contract (OpenAPI/Swagger spec)
- Deployment runbook
- Monitoring/alerting plan
- Security threat model (lightweight)

---

**End of Architect Handoff**
