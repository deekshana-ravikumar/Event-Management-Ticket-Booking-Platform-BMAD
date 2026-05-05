# 09 — Non-Functional Requirements (NFR)

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0
**Date:** 2026-05-05

> Each NFR carries an ID `NFR-NNN` and is testable. NFRs govern Architecture, QA, and DevOps.

---

## 1. Security

| ID | Requirement | Target / Standard |
|----|-------------|-------------------|
| NFR-001 | Passwords stored using **BCrypt** (work factor ≥ 10) | Verified by code review |
| NFR-002 | Authentication via **JWT access token + Refresh token** | Access exp 30 min idle; Refresh exp 7 days |
| NFR-003 | Idle session timeout | 30 minutes |
| NFR-004 | All HTTP traffic served over **HTTPS** with HSTS | Free Let's Encrypt cert; HSTS max-age=31536000 |
| NFR-005 | OWASP Top 10 baseline mitigations enforced | SQLi, XSS, CSRF, SSRF, deserialization, broken authn, etc. |
| NFR-006 | All DB access uses parameterized queries / ORM | No string-concatenated SQL anywhere |
| NFR-007 | All HTML output context-encoded | Angular auto-escaping enabled; CSP header set |
| NFR-008 | CSRF tokens on all state-changing forms (or `SameSite=Strict` cookies + JWT in Authorization header pattern) | Either pattern acceptable |
| NFR-009 | API rate limiting on sensitive endpoints | Login: 10/min/IP; Forgot-password: 5/min/IP; Coupon-apply: 30/min/user; QR-scan: 60/min/staff |
| NFR-010 | Server-side authorization on every API call | Role + ownership check; tested by automated tests |
| NFR-011 | File upload validation | MIME-type sniff, extension whitelist, size cap (banner ≤ 2 MB, docs ≤ 5 MB) |
| NFR-012 | Sensitive organizer files served via authenticated download endpoint | No public URL; Super Admin only |
| NFR-013 | QR token signed with **HMAC-SHA256** | Server-side secret rotated annually with key-id in token |
| NFR-014 | Disk-level encryption on production VPS | Provider-default LUKS / equivalent |
| NFR-015 | Secrets stored in environment variables / `.env` (git-ignored) | Production `.env` file owner-only `chmod 600` |
| NFR-016 | Audit log entries are append-only | DB constraint: no UPDATE/DELETE on audit table |
| NFR-017 | Failed-login attempts logged with IP + user-agent | For incident analysis |
| NFR-018 | Password reset and email verification tokens single-use | Marked consumed after first use |

## 2. Performance

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-101 | Page load time | < 3 sec at P95 |
| NFR-102 | API response time | < 1 sec at P95 |
| NFR-103 | Booking confirmation end-to-end | < 2 sec at P95 |
| NFR-104 | QR scan validation | < 1 sec at P95 |
| NFR-105 | Search query response | < 1 sec at P95 for typical filter combinations |
| NFR-106 | Email queue dispatch latency | < 60 sec from trigger to send attempt |

## 3. Scalability & Concurrency

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-201 | Concurrent users supported (V1) | 500+ |
| NFR-202 | Concurrent booking transactions | Moderate (e.g., 50 concurrent confirmations) without overselling |
| NFR-203 | DB-level inventory locking | Mandatory (row lock or `SELECT … FOR UPDATE`) |
| NFR-204 | Architecture is modular and stateless at API layer | Enables future horizontal scaling |
| NFR-205 | No reliance on Redis / external cache in V1 | Cache layer designed-in but not wired |

## 4. Availability & Reliability

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-301 | Uptime SLO | ≥ 99% / month (~7 hrs/month allowed) |
| NFR-302 | Daily DB backups | Retained 30 days; stored on separate volume / off-host |
| NFR-303 | Manual disaster recovery acceptable | RTO ≤ 4 hrs; RPO ≤ 24 hrs |
| NFR-304 | Background job retries | 3 retries with exponential backoff |
| NFR-305 | Health check endpoint | `/health` returns DB + SMTP status |
| NFR-306 | Graceful degradation | Discovery / dashboards remain readable if SMTP down (only sends fail) |

## 5. Compliance & Legal

| ID | Requirement | V1 Behavior |
|----|-------------|-------------|
| NFR-401 | Data retention | Transactional records retained long-term; deactivated profiles anonymized but bookings preserved |
| NFR-402 | Privacy Policy + T&C pages | Static pages live before launch |
| NFR-403 | Cookie / privacy notice banner | Simple "Accept" banner |
| NFR-404 | Booking confirmation acts as receipt | No formal GST invoice generation in V1 |
| NFR-405 | No GDPR or international compliance scope | India-only V1 |

## 6. Accessibility & Device Support

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-501 | Accessibility standard | Best-effort WCAG 2.1 AA — semantic HTML, alt text, keyboard navigation, color contrast 4.5:1 |
| NFR-502 | Browser support | Chrome, Edge, Firefox, Safari (latest 2 versions) |
| NFR-503 | Mobile responsive | All attendee + check-in pages usable on 360px width |
| NFR-504 | No native mobile app | V1 is web-only; PWA-friendly responsive design |

## 7. Localization

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-601 | UI Language | English only |
| NFR-602 | Codebase i18n-ready | All user-facing strings via message keys (Angular i18n / .resx) |
| NFR-603 | Currency format | INR with Indian comma format (e.g., ₹1,00,000.00) |
| NFR-604 | Timezone | All datetimes displayed as IST with explicit "IST" suffix |
| NFR-605 | Date format | `dd-MMM-yyyy HH:mm IST` |

## 8. Logging & Observability

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-701 | Structured JSON logs | All app logs in JSON with correlation/request-id |
| NFR-702 | Log categories | auth, booking, admin actions, QR scans, SMTP results, errors |
| NFR-703 | Log retention | 30 days rolling on disk |
| NFR-704 | Health check endpoint | Required (NFR-305) |
| NFR-705 | Free uptime monitoring (e.g., UptimeRobot) pings `/health` every 5 min | Yes |
| NFR-706 | Error monitoring | Sentry free tier or equivalent for exception tracking |

## 9. Deployment & DevOps

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-801 | Environments | Local Dev + Staging + Production |
| NFR-802 | Hosting | Low-cost VPS (e.g., Hetzner, DO, AWS Lightsail) |
| NFR-803 | Containerization | Docker images for backend + frontend; `docker-compose` for staging |
| NFR-804 | CI/CD | GitHub Actions: lint → test → build → deploy on push to `main` |
| NFR-805 | Secrets management | `.env` (git-ignored); rotation procedure documented |
| NFR-806 | Database migrations | Versioned (e.g., EF Core migrations); applied automatically on deploy with rollback path |
| NFR-807 | Zero-downtime deployment (V1 best-effort) | Rolling restart acceptable; brief blip OK within 99% uptime |

## 10. Maintainability

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-901 | Code follows ASP.NET Core + Angular standard conventions | Linter (ESLint, Roslyn analyzers) enforced in CI |
| NFR-902 | Unit test coverage | ≥ 60% on business-critical modules (Booking, Inventory, QR, Coupon, Auth) |
| NFR-903 | API documentation | OpenAPI / Swagger auto-generated, kept in sync |
| NFR-904 | Database ER diagram | Maintained as living artifact (Document 11) |
| NFR-905 | Storage abstraction (`IStorageProvider`) | Local now, cloud-swappable later |

## 11. Operational Limits / Caps

| ID | Requirement | Limit |
|----|-------------|-------|
| NFR-1001 | Banner image | ≤ 2 MB |
| NFR-1002 | Gallery images | ≤ 3 per event, ≤ 2 MB each |
| NFR-1003 | Organizer document upload | ≤ 5 MB each |
| NFR-1004 | Ticket categories per event | ≤ 10 |
| NFR-1005 | Tickets per booking | ≤ 10 |
| NFR-1006 | Description rich-text | ≤ 3000 characters |
| NFR-1007 | Review comment | ≤ 500 characters |
| NFR-1008 | CSV export size | Streamed; no in-memory cap (or ≤ 50K rows hard cap) |

---

**End of NFRs**
