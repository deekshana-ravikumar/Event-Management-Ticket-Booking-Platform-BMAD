# 04 — Module Breakdown

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0
**Date:** 2026-05-05

---

## 1. Module Topology

```
┌──────────────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER (Angular)                      │
│  Public Site │ Attendee Portal │ Organizer Portal │ Admin Console │  │
│             Check-in Web Scanner (mobile-responsive)                  │
└──────────────────────────────────────────────────────────────────────┘
                                  │ HTTPS / JWT
┌──────────────────────────────────────────────────────────────────────┐
│                  API GATEWAY / ASP.NET CORE WEB API                   │
│  Auth │ Authz │ Rate Limit │ CORS │ Request Logging │ Health        │
└──────────────────────────────────────────────────────────────────────┘
                                  │
 ┌─────────────┬─────────────┬─────────────┬─────────────┬───────────┐
 │  IDENTITY   │  ORGANIZER  │   EVENT     │  BOOKING    │ CHECK-IN  │
 │   Module    │   Module    │   Module    │   Module    │  Module   │
 └─────────────┴─────────────┴─────────────┴─────────────┴───────────┘
 ┌─────────────┬─────────────┬─────────────┬─────────────┬───────────┐
 │   TICKET    │   COUPON    │  QR/TICKET  │  REVIEW     │ REPORTING │
 │   Module    │   Module    │   Module    │   Module    │  Module   │
 └─────────────┴─────────────┴─────────────┴─────────────┴───────────┘
 ┌─────────────┬─────────────┬─────────────┬───────────────────────┐
 │NOTIFICATION │   AUDIT     │   STORAGE   │   SCHEDULER / JOBS    │
 │   Module    │   Module    │   Module    │   (Hangfire/Quartz)   │
 └─────────────┴─────────────┴─────────────┴───────────────────────┘
                                  │
                          ┌───────────────┐
                          │     MySQL     │
                          └───────────────┘
                                  │
                       ┌──────────────────────┐
                       │  Local File Storage  │
                       │   SMTP Provider      │
                       └──────────────────────┘
```

---

## 2. Module Catalog

### M01. Identity & Access Module
**Purpose:** Authentication, registration, profile, session, and role management.
**Sub-modules:**
- Registration (Attendee, Organizer)
- Email verification
- Login / JWT issuance / refresh
- Password reset & lockout
- Profile management
- Account lifecycle state engine
**Dependencies:** Notification (verification & reset emails), Audit.

### M02. Organizer Onboarding & Approval Module
**Purpose:** Govern organizer lifecycle from signup to active state, including approvals, suspensions, and sensitive-field re-reviews.
**Sub-modules:**
- Approval queue
- Decision engine (Approve / Reject / Resubmit)
- Sensitive-field change handler
- Suspension / Reactivation
- Verified Organizer badge
**Dependencies:** Identity, Notification, Audit, Storage (document uploads).

### M03. Event Management Module
**Purpose:** Event creation, validation, scheduling, lifecycle transitions, and edit governance.
**Sub-modules:**
- Event CRUD
- Pre-publish validator
- Event lifecycle state engine
- Edit-impact classifier (free / notify / restricted)
- Auto-completion scheduler
**Dependencies:** Ticket Inventory, Storage (banner/gallery), Notification (event change emails), Audit.

### M04. Ticket Inventory Module
**Purpose:** Manage ticket categories, pricing, and atomic inventory operations.
**Sub-modules:**
- Category CRUD
- Inventory ledger
- Atomic decrement / restore engine (DB-locked)
- Sold-out flag derivation
**Dependencies:** Event, Booking.

### M05. Discovery & Search Module
**Purpose:** Public event listing, search, filtering, and category/city pages.
**Sub-modules:**
- Listing service
- Search & filter engine
- Sort engine
- Private-event direct-link resolver
**Dependencies:** Event, Ticket Inventory (for free/paid + sold-out).

### M06. Booking Module
**Purpose:** Cart, multi-category booking, atomic confirmation, ticket issuance, cancellation.
**Sub-modules:**
- Cart / line-item builder
- Coupon-application orchestrator
- Confirmation transaction (inventory + booking + tickets + QR + audit)
- Cancellation handler
- Booking history queries
**Dependencies:** Identity, Ticket Inventory, Coupon, QR Ticketing, Notification, Audit.

### M07. Coupon Module
**Purpose:** Coupon definition, validation, redemption tracking.
**Sub-modules:**
- Coupon CRUD (organizer + super admin)
- Validation engine (eligibility, limits, applicability)
- Redemption tracker
- Reporting view
**Dependencies:** Booking, Audit.

### M08. QR / E-Ticket Module
**Purpose:** Generate, sign, deliver, and verify QR tokens.
**Sub-modules:**
- HMAC token signer / verifier
- QR image generator
- PDF e-ticket renderer
- Re-download handler
**Dependencies:** Booking, Notification (PDF attach), Storage (PDF cache optional).

### M09. Check-in Module
**Purpose:** Web QR scanner, validation engine, manual lookup, audit.
**Sub-modules:**
- Browser scanner (Angular component)
- Validation API (token + event + state + time-window checks)
- Manual lookup API
- Scan audit logger
**Dependencies:** QR Ticketing, Booking, Event, Identity (staff role gate), Audit.

### M10. Notification Module
**Purpose:** SMTP delivery of all transactional, lifecycle, and reminder emails.
**Sub-modules:**
- Template registry (HTML)
- SMTP sender with retry
- Reminder scheduler (24-hr, 2-hr, post-event feedback)
- Failure logger + admin alerts
**Dependencies:** Scheduler, Audit.

### M11. Review & Feedback Module
**Purpose:** Post-event ratings and comments.
**Sub-modules:**
- Eligibility checker (checked-in only)
- Review CRUD
- Aggregate rating calculator
- Moderation (admin remove)
**Dependencies:** Check-in (eligibility), Event, Identity.

### M12. Reporting & Dashboard Module
**Purpose:** KPI dashboards and CSV/PDF exports.
**Sub-modules:**
- Super Admin dashboard
- Organizer dashboard
- Attendee dashboard
- CSV export service
- Time-range filter
**Dependencies:** All transactional modules (read-side).

### M13. Admin Console Module
**Purpose:** Centralized super admin operations UI.
**Sub-modules:**
- Organizer manager
- Event manager (with Close-by-Admin)
- Coupon manager (global)
- Audit log viewer
- SMTP failure viewer
**Dependencies:** Organizer, Event, Coupon, Audit, Notification.

### M14. Audit Module
**Purpose:** Immutable structured log of significant state changes.
**Sub-modules:**
- Audit writer API
- Audit query API
- Retention manager
**Dependencies:** All modules (write-side).

### M15. Storage Module
**Purpose:** Abstract file persistence for uploads (org docs, event banner/gallery, e-ticket PDFs).
**Sub-modules:**
- Local file provider (V1)
- `IStorageProvider` interface (cloud-ready)
- Upload validator (MIME, size, extension)
**Dependencies:** Organizer, Event, QR/E-Ticket.

### M16. Scheduler / Background Jobs Module
**Purpose:** Time-based jobs.
**Sub-modules:**
- Event auto-completion job
- Reminder dispatch jobs (24-hr, 2-hr, post-event)
- SMTP retry queue
- Daily admin digest (optional)
- Cleanup / housekeeping jobs
**Dependencies:** Event, Booking, Notification.

### M17. Platform / Common Module
**Purpose:** Shared concerns across modules.
**Sub-modules:**
- i18n message catalog
- Error handling middleware
- Health endpoint
- Static pages (Privacy, T&C)
- Cookie banner
- Date/currency formatters
**Dependencies:** None (foundational).

---

## 3. Cross-Module Dependency Matrix

| Module ↓ depends on → | M01 | M02 | M03 | M04 | M06 | M07 | M08 | M09 | M10 | M14 | M15 | M16 |
|----------------------|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|
| M02 Organizer        | ●   |     |     |     |     |     |     |     | ●   | ●   | ●   |     |
| M03 Event            |     |     |     | ●   |     |     |     |     | ●   | ●   | ●   | ●   |
| M04 Ticket Inv       |     |     | ●   |     |     |     |     |     |     |     |     |     |
| M05 Discovery        |     |     | ●   | ●   |     |     |     |     |     |     |     |     |
| M06 Booking          | ●   |     | ●   | ●   |     | ●   | ●   |     | ●   | ●   |     |     |
| M07 Coupon           |     |     |     |     | ●   |     |     |     |     | ●   |     |     |
| M08 QR / Ticket      |     |     |     |     | ●   |     |     |     | ●   |     | ●   |     |
| M09 Check-in         | ●   |     | ●   |     | ●   |     | ●   |     |     | ●   |     |     |
| M11 Review           | ●   |     | ●   |     |     |     |     | ●   |     |     |     |     |
| M12 Reporting        |     | ●   | ●   | ●   | ●   | ●   |     | ●   | ●   | ●   |     |     |
| M13 Admin Console    |     | ●   | ●   |     |     | ●   |     |     | ●   | ●   |     |     |
| M16 Scheduler        |     |     | ●   |     | ●   |     |     |     | ●   |     |     |     |

---

## 4. Module Ownership (Suggested for Sprint Allocation)

| Module | Suggested Owner | Sprint Cluster |
|--------|------------------|----------------|
| M01 Identity, M17 Platform | Backend lead | Sprint 1 |
| M02 Organizer, M14 Audit, M15 Storage | Backend dev | Sprint 2 |
| M03 Event, M04 Ticket Inv, M16 Scheduler | Backend dev + Frontend | Sprint 3 |
| M05 Discovery | Frontend lead | Sprint 4 |
| M06 Booking, M07 Coupon, M08 QR | Backend lead | Sprint 5 |
| M09 Check-in | Frontend + Backend | Sprint 6 |
| M10 Notification | Backend dev | Cross-sprint |
| M11 Review, M12 Reporting, M13 Admin Console | Full team | Sprint 7 |

---

**End of Module Breakdown**
