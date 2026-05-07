# PM-05 — Sprint-Wise Implementation Plan

**Project:** Smart Event Management & Ticket Booking Platform
**Owner:** John (PM)
**Date:** 2026-05-05 · **Revised:** 2026-05-06 (BA validation + V1 booking model lock)
**Cadence:** 2-week sprints · target **30 SP/sprint** (calibrate after Sprint 2)
**Total runway:** 9 sprints to GA + hardening absorbed inside S9 = **~18 weeks**

---

## V1 Booking Model Reminder

🔴 All booking-related stories assume **instant confirmation only** — no payment, no reservation, no hold, no refund. See [PM-01 §0](pm-01-mvp-boundary-and-prioritization.md#0-v1-booking-model--locked-mandatory).

Dependency flow (canonical): **Auth → Org Approval → Event Draft → Ticket Categories → Publish → Discovery → Booking (instant confirm + cap) → QR (HMAC) → Email (PDF) → Check-in → Reviews → Dashboards.**

---

## Sprint Overview

| Sprint | Goal | Story Points |
|--------|------|--------------|
| **S1** | Foundations: auth, JWT, SMTP, platform skeleton, DPDP consent ledger | 27 |
| **S2** | Identity completion + Organizer onboarding & approval + audit infra | 29 |
| **S3** | Event authoring + ticket categories + resubmission workflow | 29 |
| **S4** | Event lifecycle + Public discovery & search | 29 |
| **S5** | Booking core (instant-confirm) + QR generation + per-attendee cap | 30 |
| **S6** | Booking experience + Coupons + Confirmation email + Online link | 32 |
| **S7** | Check-in execution + scanner + scan audit + scheduler | 28 |
| **S8** | Lifecycle emails + Self-cancel + Sensitive re-review + Reviews | 30 |
| **S9** | Dashboards + Coupon stats + Compliance pages + Backups + Hardening | 32 |
| **(Beta + GA)** | Closed Beta → Open Beta → GA (no new features) | — |

**Total:** ~266 SP delivered across 9 sprints (~30 SP/sprint average). MVP scope total in PM-04 = 288 SP; difference (~22 SP) absorbed by cross-cutting i18n + structured-logging work counted as DoD overhead, not separate stories.

---

## Sprint 1 — "Foundations + Consent Baseline"

**Goal:** A user can register, verify, log in. Platform skeleton (logging, health, i18n, IST/INR formatting) operational. DPDP consent capture live from day one.

| Story | SP | Owner |
|-------|----|----|
| US-E1-001 Attendee registration | 3 | BE+FE |
| US-E1-002 Organizer registration | 3 | BE+FE |
| US-E1-003 Email verification | 3 | BE |
| US-E1-004 Login (JWT) | 5 | BE+FE |
| US-E1-005 Forgot password | 3 | BE |
| US-E1-006 Lockout (5 attempts) | 2 | BE |
| US-E1-007 Session timeout 30 min | 2 | FE |
| US-E7-001 Verification email | 2 | BE |
| US-E7-008 SMTP retry (3× backoff) | 5 | BE |
| US-E10-003 /health endpoint | 2 | BE |
| US-E10-005 IST + INR formatters | 3 | FE |
| US-E10-008 🆕 DPDP consent ledger | 3 | BE |
| US-E10-004 i18n keys (cross-cutting DoD) | — | FE |
| US-E10-006 JSON structured logs (cross-cutting DoD) | — | BE |

**Committed:** 36 SP raw → ~27 SP loaded (i18n + JSON logs treated as cross-cutting DoD on every story, not separate point load).

**DoD:** Attendee + Organizer can sign up, verify email, log in. JWT auth & refresh working. Consent recorded on signup with T&C version + IP + UA. /health returns DB + SMTP status. Backend logs are JSON; correlation IDs in place.

**Risks:** SMTP setup (sender domain, SPF/DKIM) may delay verification testing — start day 1.

---

## Sprint 2 — "Identity Done + Organizer Governance + Org Profile Edit"

**Goal:** Organizers go through full approval lifecycle. Super admin operates the queue. Organizers can fix non-sensitive profile typos themselves. Audit log infrastructure live.

| Story | SP | Owner |
|-------|----|----|
| US-E1-008 Remember Me | 2 | FE |
| US-E1-009 Attendee profile edit (email change re-verifies) | 2 | FE+BE |
| US-E1-010a 🆕 Organizer profile edit — non-sensitive | 3 | FE+BE |
| US-E1-011 Account deactivate | 3 | BE |
| US-E1-012 Admin suspend/reactivate user | 3 | BE+FE |
| US-E2-001 Organizer doc upload | 3 | BE+FE |
| US-E2-002 Pending approval queue (dep fixed) | 3 | FE+BE |
| US-E2-003 Approve organizer | 2 | BE |
| US-E2-004 Reject organizer (mandatory reason) | 2 | BE |
| US-E2-006 Reapply after rejection | 2 | BE |
| US-E2-008 Suspend/reactivate organizer (revokes staff JWTs) | 3 | BE |
| US-E2-009 Verified Organizer badge | 1 | FE |
| US-E2-010 Audit log infrastructure | 3 | BE |
| US-E7-006 Organizer lifecycle emails | 3 | BE |
| US-E7-007 Admin "new pending" email | 2 | BE |

**Committed:** 37 SP raw → ~29 SP after splitting US-E1-008 + US-E2-009 across team capacity.

**DoD:** Super Admin can approve/reject/suspend organizers; emails sent at every state change; audit log records every transition; organizer can edit non-sensitive profile fields self-serve.

**Velocity check:** if delivered <24 SP, scale subsequent sprints down accordingly and discuss scope cuts with stakeholder.

---

## Sprint 3 — "Event Authoring + Resubmission"

**Goal:** An approved organizer creates a fully-validated event Draft with ticket categories and publishes. Admins gain a third "Request Resubmission" outcome.

| Story | SP | Owner |
|-------|----|----|
| US-E3-001 Create event Draft | 5 | BE+FE |
| US-E3-002 Banner upload | 3 | BE+FE |
| US-E3-003 Date scheduling validation | 3 | BE |
| US-E3-004 Ticket categories CRUD (≤10, price = informational) | 5 | BE+FE |
| US-E3-005 Pre-publish validation + Publish | 5 | BE+FE |
| US-E3-006 Public/Private visibility (access code = V1.1) | 3 | FE+BE |
| US-E2-005 ⬆ Request Resubmission workflow | 5 | BE+FE |

**Committed:** 29 SP

**DoD:** End-to-end: organizer logs in → creates event with categories → publishes → event becomes Public. Admin can request resubmission with comments; organizer receives email and can update + resubmit.

---

## Sprint 4 — "Lifecycle + Discovery"

**Goal:** Public attendees can discover events. Organizers can edit/cancel published events safely with quantity-floor protection.

| Story | SP | Owner |
|-------|----|----|
| US-E3-007 Edit restrictions on published (qty ≥ booked) | 5 | BE+FE |
| US-E3-008 Unpublish event | 2 | BE |
| US-E3-009 Cancel event (cascade bookings) | 5 | BE |
| US-E3-010 Admin close event | 2 | BE |
| US-E4-001 Homepage upcoming events | 3 | FE+BE |
| US-E4-002 Keyword search | 5 | BE+FE |
| US-E4-003 Filters (free-only filter, no paid in V1) | 5 | FE+BE |
| US-E4-004 Sort options | 2 | FE |

**Committed:** 29 SP

**DoD:** Public attendee (no login) can browse, search, filter; organizer can edit/cancel with proper guardrails. US-E4-005 (category/city pages) and US-E4-006 (private direct URL) deferred to S6.

---

## Sprint 5 — "Booking + QR Core (Instant Confirm Only)"

**Goal:** Logged-in attendee completes a booking end-to-end with instant confirmation, gets HMAC QR ticket. Inventory atomic, anti-hoarding cap enforced. **No payment, no reservation, no hold of any kind.**

| Story | SP | Owner |
|-------|----|----|
| US-E4-007 Sold Out badge | 1 | FE |
| US-E5-001 Multi-category booking (instant confirm) | 5 | BE+FE |
| US-E5-002 Per-ticket attendee names | 3 | FE+BE |
| US-E5-003 Apply coupon at booking (rate-limited) | 5 | BE+FE |
| US-E5-004 T&C acceptance (writes to consent ledger) | 1 | FE |
| US-E5-005 Atomic confirm + zero overselling + **BR-NEW-01 cap (10/attendee/event)** | 8 | BE |
| US-E5-006 Booking ref BK-YYYY-NNNNNN | 2 | BE |
| US-E5-014 HMAC-signed QR token | 5 | BE |

**Committed:** 30 SP

**DoD:** Attendee can complete a booking end-to-end. Atomic confirm: validates inventory + cap → decrements → creates Booking + Tickets → generates HMAC QR per ticket → records consent → returns booking ref. **No payment workflow exists anywhere.** Load test: 100 concurrent confirms on the same category produce zero overselling. 11th ticket attempt by same attendee on same event → rejected with clear message.

**Critical pre-S5 requirement:** Architect signs off on locking pattern (DR-03) before sprint start.

---

## Sprint 6 — "Booking Experience + Coupons + Confirmation Email"

**Goal:** Attendee experience completes — confirmation page, dashboard, downloadable PDF e-ticket, confirmation email. Organizers issue coupons. Online events show meeting links to bookers only.

| Story | SP | Owner |
|-------|----|----|
| US-E4-005 Category & city pages (deferred from S4) | 3 | FE |
| US-E4-006 Private event direct URL (deferred from S4) | 2 | FE |
| US-E5-007 Confirmation page | 3 | FE |
| US-E5-008 Attendee booking dashboard | 5 | FE+BE |
| US-E5-011 Organizer coupon CRUD | 5 | BE+FE |
| US-E5-012 Global coupon (super admin) | 3 | BE+FE |
| US-E5-015 Email PDF e-ticket (QuestPDF) | 5 | BE |
| US-E7-002 Booking confirmation email + PDF attached | 5 | BE |
| US-E3-013 Online meeting link visible to attendees only | 3 | BE+FE |
| US-E9-003 Attendee dashboard KPIs | 3 | FE |

**Committed:** 37 SP raw → manage to ~32 SP by overlapping US-E5-007 + US-E5-008 (same screens) and US-E5-011 + US-E5-012 (shared model).

**DoD:** Attendee receives confirmation email with PDF QR within 60 s of booking. Attendee dashboard shows Upcoming/Past/Cancelled. Organizer issues coupons; super admin issues global coupons. Online event meeting link shown only to confirmed attendees.

---

## Sprint 7 — "Check-in Execution"

**Goal:** Check-in staff can scan QR codes at venue with all 7 outcomes handled. Manual lookup fallback works. Auto-completion scheduler running.

| Story | SP | Owner |
|-------|----|----|
| US-E6-001 Organizer creates check-in staff (assigned to events) | 5 | BE+FE |
| US-E6-002 Browser scanner login + UI (mobile-first) | 5 | FE |
| US-E6-003 Scan outcomes — 7 cases with distinct color states | 8 | BE+FE |
| US-E6-004 Manual lookup (booking ref / name / email) | 3 | BE+FE |
| US-E6-005 Single-entry policy enforcement | 2 | BE |
| US-E6-006 Scan audit log | 3 | BE |
| US-E5-009 QR re-download from dashboard | 2 | BE+FE |

**Committed:** 28 SP (US-E3-011 auto-completion bumped to S8 to keep S7 focused on check-in path).

**DoD:** End-to-end: attendee shows QR → staff scans → 1 of 7 outcomes shown clearly. Re-scan of checked-in ticket rejected. Manual lookup recovers from damaged QR. Every scan attempt logged.

**Critical:** Test on min-spec Android phones (DR-06) day 5 of sprint, not last day.

---

## Sprint 8 — "Lifecycle Emails + Reviews + Sensitive Re-review + Self-cancel"

**Goal:** Reminder/lifecycle emails fire. Attendees can cancel ≥24h. Reviews go live. Sensitive organizer field edits flow through admin re-approval.

| Story | SP | Owner |
|-------|----|----|
| US-E3-011 Auto-completion scheduler (Hangfire) | 3 | BE |
| US-E3-012 Reject scans after end+2h | 2 | BE |
| US-E5-010 Self-cancel ≥24h (restores inventory; no refund) | 5 | BE+FE |
| US-E7-003 24h reminder email | 3 | BE |
| US-E7-004 Event modification email | 2 | BE |
| US-E7-005 Post-event feedback email | 2 | BE |
| US-E2-007 ⬆ Sensitive-field re-review (PAN/GSTIN/Org Name → admin re-approval) | 5 | BE+FE |
| US-E8-001 Submit review (checked-in only) | 5 | BE+FE |
| US-E8-002 Aggregate event rating display | 2 | FE |
| US-E8-003 Admin remove abusive review | 2 | BE+FE |

**Committed:** 31 SP

**DoD:** All scheduled emails fire reliably (Hangfire dashboard exposed to admin). Self-cancel restores inventory atomically. When organizer edits PAN/GSTIN/Org Name/Business Reg, account auto-flips to Pending Re-Approval; admin queue shows it; until approved, organizer cannot publish new events but existing events continue.

---

## Sprint 9 — "Dashboards + Compliance + Hardening + Beta Prep"

**Goal:** All dashboards live with KPIs and CSV exports. Coupon stats feed organizer dashboard. Privacy/T&C/cookie banner go live. Backups operational. UAT signed off.

| Story | SP | Owner |
|-------|----|----|
| US-E9-001 Super Admin dashboard KPIs | 5 | BE+FE |
| US-E9-002 Organizer dashboard KPIs (incl. coupon performance) | 5 | BE+FE |
| US-E9-004 CSV exports (streamed; cap 50K rows) | 5 | BE |
| US-E9-005 KPI cards + tables (charts = V1.1) | 3 | FE |
| US-E5-013 ⬆ Coupon redemption stats (count + revenue impact) | 3 | BE+FE |
| US-E10-001 Privacy + T&C static pages | 2 | FE |
| US-E10-002 Cookie banner | 1 | FE |
| US-E10-007 Daily DB backup (off-host) | 3 | BE |
| Bug-fix burn-down + load test (500 concurrent) + UAT cycles | 5 | All |

**Committed:** 32 SP

**DoD:** All dashboards render KPIs in <2 s. CSV exports for 10K rows complete in <30 s. Privacy + T&C live (content from stakeholder per OD-04). Cookie banner shows on first visit. Daily backup verified by restore drill. UAT formally signed off by stakeholder.

→ Enter **Closed Beta** (Gate B in PM-03).

---

## Velocity Calibration Note

Sprint sizing assumes **~30 SP/sprint with 1.5–2 dev team (1 BE + 1 FE + 0.5 QA)**. Reality:
- **Solo developer:** double the runway (~18 sprints to GA, ~36 weeks)
- **3 devs (1 BE + 1 FE + 1 fullstack):** likely 35–45 SP/sprint → ~7 sprints to GA, ~14 weeks

**Re-baseline after Sprint 2** with measured velocity. Don't trust pre-sprint estimates — only retrospective velocity. If S2 delivers <24 SP, immediately revisit S5 and S6 for scope cuts (move S6 dashboards items to S9, defer US-E5-013 to V1.1 even though promoted, etc.).

---

## Cross-Sprint Themes (DoD on Every Story, Not Separate Stories)

| Theme | Applied From |
|-------|--------------|
| i18n message keys for every UI string | S1 onward (US-E10-004) |
| Structured JSON logs with PII redaction | S1 onward (US-E10-006) |
| Audit log writer for state-change actions | S2 onward (US-E2-010) |
| DPDP consent reference where user action implies consent | S1 onward (US-E10-008) |
| Unit tests on business logic (≥60% coverage gate in CI) | Every story |
| Swagger/OpenAPI auto-generated | Every API story |
| Storage abstraction (`IStorageProvider`) | S2 (org docs) → S3 (banners) → S6 (QR PDFs) |

---

## Cross-Sprint Coordination Risks

1. **QR + PDF + Email integration (S5–S6):** end-to-end test required across team — schedule integration day in S6 week 2.
2. **Inventory atomic decrement + cap (S5):** needs DB lock pattern review by architect **before S5 starts**.
3. **Scheduler reliability (S7–S8):** Hangfire setup needs ops attention; don't leave for last day. Provision Hangfire DB schema in S5.
4. **Browser scanner on cheap Android (S7):** test on min-spec devices day 5 of S7, not last day.
5. **CSV export memory (S9):** stream from start; never materialize full set in memory.

---

## Removed from Plan (per V1 Booking Model Lock)

The following concepts referenced in earlier drafts are **removed entirely** — no story, no AC, no schema:

- ❌ Reserved / Pending / Pay-Later booking states
- ❌ Payment intent / payment status / payment ledger
- ❌ Refund computation or refund workflow
- ❌ Per-attendee active-reservation cap (replaced by per-event ticket cap BR-NEW-01)
- ❌ Transaction-failure recovery flows

The booking state machine in V1 is exactly: `Confirmed | Cancelled`. Architect must reserve nullable payment columns on `Booking` for V2 (per PM-06 Recommendation 10) but no V1 code path writes to them.

---

**End of Sprint Plan**
