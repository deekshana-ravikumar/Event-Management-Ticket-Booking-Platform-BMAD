# 01 — Project Scope Baseline

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0
**Date:** 2026-05-05
**Status:** Locked

---

## 1. Vision Statement

A centralized, low-cost, scalable web platform that enables organizations and event hosts to digitally create, publish, manage, and operate events — and allows attendees to discover events, book tickets, receive secure QR-based e-tickets, and check in seamlessly at the venue.

V1 prioritizes **operational completeness over monetization**, establishing a production-ready foundation for future revenue and enterprise features.

---

## 2. V1 Strategic Objectives

| # | Objective | Success Indicator (6-Month) |
|---|-----------|------------------------------|
| O1 | Enable organizers to digitally manage end-to-end event lifecycle | Successful event publishing count |
| O2 | Provide attendees a frictionless discovery → booking → check-in flow | Attendee registrations + bookings count |
| O3 | Deliver secure QR-based digital ticketing with reliable validation | Successful QR validations / check-ins |
| O4 | Maintain stable, low-cost operations on free/open-source stack | Platform operational stability (≥99% uptime) |
| O5 | Build a payment-ready, scalable architecture for Phase 2 | Schema and APIs accept price metadata without rework |

---

## 3. In Scope (V1) — Must Have

### 3.1 Identity & Access
- Attendee self-registration (email + password) with email verification
- Organizer self-registration with email verification + Super Admin approval workflow (Approve / Reject / Request Resubmission)
- Check-in Staff accounts created by Organizer, scoped to assigned events
- Single centralized Super Admin role
- Password policy enforcement, 5-attempt lockout (15-min), forgot-password via SMTP
- JWT auth with refresh token, 30-min idle session timeout

### 3.2 Organizer Management
- Pending Approval Queue with sort & SLA visibility (48-hr target)
- Three-outcome admin review (Approve / Reject / Resubmission Request)
- Sensitive-field re-review (Org Name, PAN, GSTIN, business docs)
- Suspension / Reactivation with mandatory reason + audit trail
- "Verified Organizer" badge on public profile

### 3.3 Event Management
- Event creation with Offline / Online / Hybrid types
- Mandatory + conditional fields (venue for offline/hybrid, meeting link for online/hybrid)
- Multi-day event support (start/end datetime range)
- Event lifecycle: Draft → Published → Unpublished / Cancelled / Completed / Closed by Admin
- Pre-publish validation; direct publish (no per-event admin approval)
- Edit restrictions on published events with attendee notification on critical changes
- Auto-completion scheduler (event end + 2-hour grace)

### 3.4 Ticket Inventory
- Up to 10 ticket categories per event
- Per-category pricing, quantity, min/max booking, sale window
- Free booking + Reserved/Pay-Later booking modes (payment-gateway-ready)
- Atomic inventory decrement with DB-level locking (zero overselling)
- Sold Out derived flag

### 3.5 Discovery & Search
- Public / Private (Unlisted) event visibility
- Optional access code for private events
- Home, Category, City listing pages
- Search filters: keyword, category, city, date range, free/paid, online/offline, language
- Sort: soonest / newest / A–Z

### 3.6 Booking & Ticketing
- Logged-in attendee booking only
- Multi-category in single cart
- Per-attendee name capture for individual QR issuance
- Booking reference format `BK-YYYY-000001`
- Booking statuses (Confirmed, Cancelled by *, Completed, Expired)
- Per-ticket statuses (Issued, Checked In, Cancelled, Expired)
- Self-cancellation up to 24 hours before event start (returns inventory)
- ICS calendar download
- One unique HMAC-signed QR per ticket
- QR delivery: confirmation page + emailed PDF + dashboard re-download

### 3.7 Coupons
- Organizer-level + Super Admin global coupons
- Flat / Percentage discount with cap, min booking amount, validity, total + per-user usage limits
- One coupon per booking, no stacking, blocked on free bookings
- Redemption analytics

### 3.8 Check-in
- Browser-based mobile-friendly QR scanner (camera API)
- Scan outcomes: Valid, Already Checked-In, Invalid, Wrong Event, Cancelled, Expired, Not Yet Active
- Manual lookup (booking ref / name / email)
- Single-entry policy
- Full check-in audit log

### 3.9 Notifications (SMTP only)
- Comprehensive transactional email catalog (attendee, organizer, super admin, check-in staff)
- HTML branded template
- 3-attempt SMTP retry + failure logging

### 3.10 Reviews
- Checked-in attendees only, 1–5 stars + 500-char comment
- Auto-publish; Super Admin can remove abusive reviews
- Aggregated rating on event + organizer profile

### 3.11 Reports & Dashboards
- Super Admin: organizers, events, bookings, attendees, check-ins, coupons, failed emails
- Organizer: my events, bookings, sold/available, expected revenue, check-ins, coupon perf, attendee export, ratings
- Attendee: upcoming, past, cancelled, attended count
- CSV exports (attendee list, booking list, check-in audit, coupon usage, organizer event report)
- KPI cards + tables + basic open-source charts

### 3.12 Platform Pages
- Static Privacy Policy & Terms pages
- Cookie / privacy notice banner
- Health check endpoint

---

## 4. Out of Scope (V1) — Deferred to Phase 2+

| Capability | Rationale |
|------------|-----------|
| Online payment gateway integration | Cost + scope discipline; price metadata captured for forward compatibility |
| Automated refund workflow | Depends on payments |
| SMS notifications | Paid provider |
| Push notifications | Paid provider / app-store dependency |
| Advanced seat-map selection | Complexity; not required by target use cases |
| Organizer monetization plans (subscriptions, commissions, premium promotions) | No revenue focus in V1 |
| White-labeled organizer portals / sub-domains | Single-brand V1 |
| Native mobile apps (attendee or check-in) | Web suffices; PWA-friendly responsive design |
| Google / Apple SSO | Email/password sufficient for V1 |
| Recurring / series events | One-off events only in V1 |
| Waitlist for sold-out events | Defer |
| Invite-only events with CSV upload | Private (unlisted) URL covers most needs |
| Offline check-in scanning | Online connectivity assumed |
| Geo-map / venue master library | Free-text venue fields only |
| Multi-language UI (Hindi, Tamil, etc.) | i18n-ready codebase, content English-only |
| Multi-country / multi-currency | India / INR only |
| RBAC sub-roles for admin | Single Super Admin role |
| Custom organizer-defined booking questions | Skip in V1 |
| Advanced GST invoicing | Booking confirmation acts as receipt |
| Hot-standby DR | Manual restore acceptable |

---

## 5. Geography, Locale & Currency

| Dimension | V1 Setting |
|-----------|------------|
| Target Market | India only |
| Currency | INR (₹) |
| Currency Format | Indian comma format (e.g., ₹1,00,000.00) |
| Language (UI) | English |
| Codebase i18n | Required (message-key based) |
| Timezone | IST (Asia/Kolkata) — displayed explicitly |
| Date Format | `dd-MMM-yyyy HH:mm IST` |

---

## 6. Scale Assumptions (Year 1)

| Metric | V1 Target |
|--------|-----------|
| Active Organizers | 100+ |
| Events / Month | 500+ |
| Registered Attendees | 10,000+ |
| Concurrent Users (P95) | 500 |
| Concurrent Bookings | Moderate (zero-oversell guarantee) |
| Uptime SLO | 99% |
| Booking confirmation latency (P95) | < 2 sec |
| QR scan validation latency (P95) | < 1 sec |

---

## 7. Tech Stack (Locked)

- **Frontend:** Angular
- **Backend:** ASP.NET Core Web API
- **Database:** MySQL
- **Auth:** JWT + Refresh Tokens
- **Storage:** Local filesystem (V1)
- **Email:** SMTP
- **QR:** Open-source libraries (e.g., QRCoder for .NET, ngx-scanner for Angular)
- **Background Jobs:** ASP.NET Core HostedService / Quartz.NET / Hangfire (free tier)
- **CI/CD:** GitHub Actions
- **Hosting:** Low-cost VPS (Docker-based deployment)

---

## 8. Stakeholders & Roles

| Stakeholder | Role | Primary Interest |
|-------------|------|------------------|
| Super Admin | Platform owner / operator | Governance, organizer approval, platform health |
| Event Organizer / Host | Tenant | Event creation, attendee management, reporting |
| Customer / Attendee | End user | Discovery, booking, check-in experience |
| Event Check-in Staff | Operator | Fast, reliable on-site validation |

---

## 9. Risks & Constraints

| ID | Risk / Constraint | Mitigation |
|----|--------------------|-----------|
| R1 | No payment gateway in V1 may limit organizer adoption | "Reserved / Pay Later" mode; clear Phase 2 roadmap |
| R2 | SMTP-only notifications may have deliverability issues | Use reputable SMTP provider; retry; log failures |
| R3 | Local file storage limits scale | Abstract via storage interface for cloud swap |
| R4 | Single Super Admin = single point of operational failure | Phase 2 RBAC; audit log all admin actions |
| R5 | Browser-based check-in scanner depends on venue Wi-Fi | Document requirement; offline mode in Phase 2 |
| R6 | QR token security relies on secret key custody | Vault rotation procedure; HMAC SHA-256 |

---

## 10. Phase 2 Forward-Compatibility Promises

The V1 architecture must:
- Accept price metadata so payment gateway integrates without schema change
- Use a `StorageProvider` interface (local now, S3-compatible later)
- Use `NotificationChannel` abstraction (SMTP now, SMS / push later)
- Use i18n message catalogs (English now, multilingual later)
- Use modular event lifecycle services (recurring events drop in cleanly)
- Reserve organizer `tier` / `subscription_plan` columns (nullable) for future monetization

---

**End of Project Scope Baseline**
