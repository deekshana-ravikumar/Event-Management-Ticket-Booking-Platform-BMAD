# PM-05 — Sprint-Wise Implementation Plan

**Project:** Smart Event Management & Ticket Booking Platform
**Owner:** John (PM)
**Date:** 2026-05-05
**Cadence:** 2-week sprints · ~25 SP/sprint capacity (calibrate after Sprint 2)

---

## Sprint Overview

| Sprint | Goal | Story Points |
|--------|------|--------------|
| **S1** | Foundations: auth, JWT, SMTP, platform skeleton | 24 |
| **S2** | Identity completion + Organizer onboarding & approval | 26 |
| **S3** | Event authoring + ticket categories | 24 |
| **S4** | Event lifecycle + Public discovery & search | 24 |
| **S5** | Booking, QR generation, coupon engine | 31 |
| **S6** | Check-in, scheduler, all email workflows complete | 30 |
| **S7** | Reviews + Dashboards + CSV exports | 22 |
| **S8** | Hardening, static pages, backups, UAT, beta prep | 13 + buffer |
| **(Beta + GA)** | Closed Beta → Open Beta → GA (no new features) | — |

Total MVP delivery: **~16 weeks of build + 4 weeks beta/GA = ~20 weeks** (calibrate after S2 velocity reading).

---

## Sprint 1 — "Foundations"

**Sprint Goal:** A user can register, verify, log in, and the platform skeleton (logging, health, i18n, IST/INR formatting) is operational.

| Story | SP | Owner |
|-------|----|----|
| US-E1-001 Attendee registration | 3 | BE+FE |
| US-E1-002 Organizer registration | 3 | BE+FE |
| US-E1-003 Email verification | 3 | BE |
| US-E1-004 Login (JWT) | 5 | BE+FE |
| US-E1-005 Forgot password | 3 | BE |
| US-E1-006 Lockout | 2 | BE |
| US-E1-007 Session timeout | 2 | FE |
| US-E7-001 Verification email | 2 | BE |
| US-E7-008 SMTP retry queue | 5 | BE |
| US-E10-003 /health endpoint | 2 | BE |
| US-E10-005 IST + INR formatters | 3 | FE |
| US-E10-004 i18n keys (cross-cutting kickoff) | 5 (parked) | FE |
| US-E10-006 JSON structured logs (cross-cutting kickoff) | 5 (parked) | BE |

**Sprint 1 committed:** 24 SP (i18n + logging done as cross-cutting infra during sprint)

**Definition of Done for Sprint:**
- Attendee + Organizer can sign up, verify email, log in
- JWT auth & refresh working
- Forgot-password loop works end-to-end
- /health returns DB + SMTP status
- Backend logs are JSON; correlation IDs in place

**Risks:** SMTP setup (sender domain, SPF/DKIM) might delay verification testing — start day 1.

---

## Sprint 2 — "Identity Done + Organizer Governance"

**Sprint Goal:** Organizers go through full approval lifecycle; super admin operates the queue.

| Story | SP | Owner |
|-------|----|----|
| US-E1-008 Remember Me | 2 | FE |
| US-E1-009 Attendee profile edit | 2 | FE+BE |
| US-E1-011 Account deactivate | 3 | BE |
| US-E1-012 Admin suspend/reactivate | 3 | BE+FE |
| US-E2-001 Organizer doc upload | 3 | BE+FE |
| US-E2-002 Pending approval queue | 3 | FE+BE |
| US-E2-003 Approve organizer | 2 | BE |
| US-E2-004 Reject organizer | 2 | BE |
| US-E2-006 Reapply after rejection | 2 | BE |
| US-E2-008 Suspend/reactivate organizer | 3 | BE |
| US-E2-009 Verified badge (public) | 1 | FE |
| US-E2-010 Audit log infra | 3 | BE |
| US-E7-006 Organizer lifecycle emails | 3 | BE |
| US-E7-007 Admin "new pending" email | 2 | BE |

**Sprint 2 committed:** 26 SP

**DoD:** Super Admin can approve/reject/suspend organizers; emails sent at every state change; audit log records every transition.

**Velocity check at end of S2:** if delivered <22 SP, scale subsequent sprints down accordingly.

---

## Sprint 3 — "Event Authoring"

**Sprint Goal:** An approved organizer can create a fully-validated event Draft with ticket categories.

| Story | SP | Owner |
|-------|----|----|
| US-E3-001 Create event Draft | 5 | BE+FE |
| US-E3-002 Banner upload | 3 | BE+FE |
| US-E3-003 Scheduling validation | 3 | BE |
| US-E3-004 Ticket categories CRUD | 5 | BE+FE |
| US-E3-005 Pre-publish validate + Publish | 5 | BE+FE |
| US-E3-006 Public/Private visibility | 3 | FE+BE |

**Sprint 3 committed:** 24 SP

**DoD:** End-to-end: organizer logs in → creates event with categories → publishes → event becomes Public.

---

## Sprint 4 — "Lifecycle + Discovery"

**Sprint Goal:** Public attendees can discover events; organizers can edit/cancel published events safely.

| Story | SP | Owner |
|-------|----|----|
| US-E3-007 Edit restrictions on published | 5 | BE+FE |
| US-E3-008 Unpublish | 2 | BE |
| US-E3-009 Cancel event | 5 | BE |
| US-E3-010 Admin close event | 2 | BE |
| US-E4-001 Homepage upcoming | 3 | FE+BE |
| US-E4-002 Keyword search | 5 | BE+FE |
| US-E4-003 Filters | 5 | FE+BE |
| US-E4-004 Sort options | 2 | FE |
| US-E4-005 Category + City pages | 3 | FE |
| US-E4-006 Private direct URL | 2 | FE |

**Sprint 4 committed:** 34 SP — **OVER capacity**. Trim US-E4-005 to S5 if velocity demands. Target ~25.

**DoD:** Attendee (no login) can browse, search, filter; organizer can edit/cancel with proper guardrails.

---

## Sprint 5 — "Booking + Coupons + QR Gen"

**Sprint Goal:** Logged-in attendee completes a booking end-to-end and downloads QR ticket.

| Story | SP | Owner |
|-------|----|----|
| US-E4-007 Sold Out badge | 1 | FE |
| US-E5-001 Multi-category booking | 5 | BE+FE |
| US-E5-002 Per-ticket attendee names | 3 | FE+BE |
| US-E5-003 Apply coupon | 5 | BE+FE |
| US-E5-004 T&C acceptance | 1 | FE |
| US-E5-005 Atomic confirm + zero overselling | 8 | BE |
| US-E5-006 Booking ref BK-YYYY-NNNNNN | 2 | BE |
| US-E5-007 Confirmation page | 3 | FE |
| US-E5-008 Attendee bookings dashboard | 5 | FE+BE |
| US-E5-009 QR re-download | 2 | BE+FE |
| US-E5-011 Organizer coupon CRUD | 5 | BE+FE |
| US-E5-012 Global coupon | 3 | BE+FE |
| US-E5-014 HMAC-signed QR token | 5 | BE |
| US-E3-013 Online meeting link visible to bookers | 3 | BE+FE |
| US-E9-003 Attendee dashboard KPIs | 3 | FE |

**Sprint 5 committed:** 54 SP — **WAY OVER**. Reality: split across S5 and S6.

**Hard split:** S5 = US-E5-001/002/003/004/005/006/007/014 (32 SP) — focus on **booking + QR** core. The rest move to S6.

---

## Sprint 6 — "Check-in + Notifications + Scheduler"

**Sprint Goal:** Check-in staff scans QR codes; all reminder/lifecycle emails fire.

| Story | SP | Owner |
|-------|----|----|
| US-E5-008 Attendee bookings dashboard | 5 | FE+BE |
| US-E5-009 QR re-download | 2 | BE+FE |
| US-E5-010 Self-cancel ≥24h | 5 | BE+FE |
| US-E5-011 Organizer coupon CRUD | 5 | BE+FE |
| US-E5-012 Global coupon | 3 | BE+FE |
| US-E5-015 Email PDF e-ticket | 5 | BE |
| US-E6-001 Create check-in staff | 5 | BE+FE |
| US-E6-002 Browser scanner UI | 5 | FE |
| US-E6-003 Scan outcomes (7 cases) | 8 | BE+FE |
| US-E6-004 Manual lookup | 3 | BE+FE |
| US-E6-005 Single-entry policy | 2 | BE |
| US-E6-006 Scan audit | 3 | BE |
| US-E3-011 Auto-completion scheduler | 3 | BE |
| US-E3-012 Reject scans after end+2h | 2 | BE |
| US-E7-002 Booking confirmation email + PDF | 5 | BE |
| US-E7-003 24h reminder | 3 | BE |
| US-E7-004 Event modification email | 2 | BE |
| US-E7-005 Post-event feedback email | 2 | BE |

**Sprint 6 committed:** 68 SP — **needs hard split**.

Realistically S6 + S7 each take half of this. Re-balance:
- S6 = check-in + booking emails + scheduler core (35 SP target ≤30 → drop dashboard items to S7)
- S7 absorbs dashboard + reviews + remaining emails

**S6 final:** US-E5-010, E5-015, E6-001..006, E3-011, E3-012, E7-002, E7-003, E7-004, E7-005 = ~50 SP. **Over.**

This signals: **either velocity is higher than 25 SP/sprint, OR add Sprint 6.5**. Realistic call: extend MVP to **9 sprints**, push reviews/dashboards to S7+S8.

---

## Sprint 7 — "Reviews + Dashboards"

| Story | SP |
|-------|----|
| US-E8-001 Submit review | 5 |
| US-E8-002 Aggregate event rating | 2 |
| US-E8-003 Admin remove review | 2 |
| US-E9-001 Super Admin dashboard KPIs | 5 |
| US-E9-002 Organizer dashboard KPIs | 5 |
| US-E9-004 CSV exports | 5 |
| US-E9-005 KPI cards + tables | 3 |

**S7 committed:** 27 SP

---

## Sprint 8 — "Hardening + Beta Prep"

| Story | SP |
|-------|----|
| US-E10-001 Privacy + T&C pages | 2 |
| US-E10-002 Cookie banner | 1 |
| US-E10-007 Daily backup | 3 |
| Bug-fix buffer | 10 |
| Load test (500 concurrent) | 3 |
| UAT cycles + sign-off | 5 |

**S8:** dedicated to hardening; no new functional stories.

→ Enter Closed Beta (Gate B in PM-03).

---

## Velocity Calibration Note

**These sprint counts assume 25 SP/sprint with 2 devs.** Actual reality:
- If team = solo developer: **double the sprint count** (~16 sprints to GA, ~32 weeks)
- If team = 3 devs (1 BE + 1 FE + 1 fullstack): **expect 30–40 SP/sprint** (~7 sprints to GA, ~14 weeks)

**Re-baseline after Sprint 2** with measured velocity. Don't trust pre-sprint estimates — only retrospective velocity.

---

## Cross-Sprint Themes (Done Continuously)

| Theme | Sprint(s) |
|-------|-----------|
| Audit log writer | Built in S2; consumed by all subsequent sprints |
| i18n message keys | Practiced from S1, refined throughout |
| Structured logging | S1 onward |
| Unit tests on business logic | Every story (≥60% coverage gate in CI) |
| Swagger/OpenAPI | Auto-generated; verified each sprint |
| Storage abstraction (IStorageProvider) | Built in S2 (organizer docs) — reused S3 (banners) and S5 (QR PDFs) |

---

## Cross-Sprint Coordination Risks

1. **QR + PDF + Email integration (S5–S6):** end-to-end test required across team — schedule integration day.
2. **Inventory atomic decrement (S5):** needs DB lock pattern review by architect early.
3. **Scheduler reliability (S6):** Hangfire setup needs ops attention; don't leave for last day.

---

**End of Sprint Plan**
