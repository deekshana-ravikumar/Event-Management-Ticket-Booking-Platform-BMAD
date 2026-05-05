# PM-04 — Product Backlog (Refined)

**Project:** Smart Event Management & Ticket Booking Platform
**Owner:** John (PM)
**Date:** 2026-05-05
**Source:** Mary's [06-epics-and-user-stories.md](../planning-artifacts/06-epics-and-user-stories.md), refined with PM priority + sizing.

---

## Conventions

- **Pri:** P0 (MVP must-have) · P1 (MVP should) · P2 (V1.1) · P3 (V1.2+)
- **SP:** Story points (Fibonacci: 1, 2, 3, 5, 8, 13)
- **Dep:** Dependent story IDs (must complete first)
- **Sprint:** Target sprint allocation (see PM-05)

---

## Epic E1 — Identity & Access Foundation

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E1-001 | Attendee registration | P0 | 3 | — | S1 |
| US-E1-002 | Organizer registration | P0 | 3 | — | S1 |
| US-E1-003 | Email verification | P0 | 3 | E1-001, E1-002 | S1 |
| US-E1-004 | Login (JWT issuance) | P0 | 5 | E1-003 | S1 |
| US-E1-005 | Forgot password | P0 | 3 | E1-004 | S1 |
| US-E1-006 | Account lockout (5 attempts) | P0 | 2 | E1-004 | S1 |
| US-E1-007 | Session timeout 30 min | P0 | 2 | E1-004 | S1 |
| US-E1-008 | "Remember Me" | P1 | 2 | E1-004 | S2 |
| US-E1-009 | Attendee profile edit | P0 | 2 | E1-004 | S2 |
| US-E1-010 | Organizer profile edit (sensitive re-review) | P2 | 5 | E1-004, E2-007 | V1.1 |
| US-E1-011 | Account deactivation (preserve history) | P1 | 3 | E1-004 | S2 |
| US-E1-012 | Admin suspend/reactivate user | P0 | 3 | E1-004 | S2 |
| **Epic E1 total** | | | **36** | | |

---

## Epic E2 — Organizer Onboarding & Governance

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E2-001 | Organizer document upload (optional) | P0 | 3 | E1-002 | S2 |
| US-E2-002 | Pending approval queue (admin) | P0 | 3 | E1-012 | S2 |
| US-E2-003 | Approve organizer | P0 | 2 | E2-002 | S2 |
| US-E2-004 | Reject organizer (mandatory reason) | P0 | 2 | E2-002 | S2 |
| US-E2-005 | Request resubmission (3rd outcome) | P2 | 5 | E2-002 | V1.1 |
| US-E2-006 | Reapply after rejection | P0 | 2 | E2-004 | S2 |
| US-E2-007 | Sensitive-field re-review | P2 | 5 | E1-010 | V1.1 |
| US-E2-008 | Suspend / reactivate organizer | P0 | 3 | E1-012 | S2 |
| US-E2-009 | "Verified Organizer" badge | P0 | 1 | E2-003 | S2 |
| US-E2-010 | Audit log for state transitions | P0 | 3 | — | S2 |
| **Epic E2 total** | | | **29** | | |

---

## Epic E3 — Event Authoring & Lifecycle

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E3-001 | Create event Draft | P0 | 5 | E2-003 | S3 |
| US-E3-002 | Banner upload (+ gallery) | P0 (banner) / P2 (gallery) | 3 | E3-001 | S3 |
| US-E3-003 | Date scheduling validation | P0 | 3 | E3-001 | S3 |
| US-E3-004 | Ticket categories CRUD (≤10) | P0 | 5 | E3-001 | S3 |
| US-E3-005 | Pre-publish validation + Publish | P0 | 5 | E3-004 | S3 |
| US-E3-006 | Public/Private visibility (access code = P2) | P0 / P2 | 3 | E3-005 | S3 / V1.1 |
| US-E3-007 | Edit restrictions on published | P0 | 5 | E3-005 | S4 |
| US-E3-008 | Unpublish event | P0 | 2 | E3-005 | S4 |
| US-E3-009 | Cancel event (cascade bookings) | P0 | 5 | E3-005, E5-005 | S4 |
| US-E3-010 | Admin close event | P0 | 2 | E1-012, E3-005 | S4 |
| US-E3-011 | Auto-completion scheduler | P0 | 3 | E3-005 | S6 |
| US-E3-012 | Reject scans after end+2h | P0 | 2 | E3-011 | S6 |
| US-E3-013 | Online meeting link visible only to attendees | P0 | 3 | E3-001, E5-005 | S5 |
| **Epic E3 total** | | | **46** | | |

---

## Epic E4 — Public Discovery & Search

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E4-001 | Homepage upcoming events | P0 | 3 | E3-005 | S4 |
| US-E4-002 | Keyword search | P0 | 5 | E3-005 | S4 |
| US-E4-003 | Filters (cat, city, date, free/paid, type, lang) | P0 | 5 | E4-002 | S4 |
| US-E4-004 | Sort options | P0 | 2 | E4-002 | S4 |
| US-E4-005 | Category & city pages | P0 | 3 | E4-002 | S4 |
| US-E4-006 | Private event direct URL | P0 | 2 | E3-006 | S4 |
| US-E4-007 | Sold Out badge | P0 | 1 | E5-005 | S5 |
| **Epic E4 total** | | | **21** | | |

---

## Epic E5 — Booking, Coupons & Ticketing

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E5-001 | Multi-category booking | P0 | 5 | E3-004 | S5 |
| US-E5-002 | Per-ticket attendee names | P0 | 3 | E5-001 | S5 |
| US-E5-003 | Apply coupon at booking | P0 | 5 | E5-001, E5-011 | S5 |
| US-E5-004 | T&C acceptance | P0 | 1 | E5-001 | S5 |
| US-E5-005 | Atomic confirm + zero overselling | P0 | 8 | E5-001 | S5 |
| US-E5-006 | Booking reference `BK-YYYY-NNNNNN` | P0 | 2 | E5-005 | S5 |
| US-E5-007 | Confirmation page + ICS (P2) | P0 / P2 | 3 | E5-005 | S5 / V1.1 |
| US-E5-008 | Attendee booking dashboard | P0 | 5 | E5-005 | S5 |
| US-E5-009 | QR re-download | P0 | 2 | E5-014 | S5 |
| US-E5-010 | Self-cancel ≥24h | P0 | 5 | E5-005 | S6 |
| US-E5-011 | Organizer coupon CRUD | P0 | 5 | E2-003 | S5 |
| US-E5-012 | Global coupon (super admin) | P0 | 3 | E1-012 | S5 |
| US-E5-013 | Coupon redemption stats | P2 | 3 | E5-011 | V1.1 |
| US-E5-014 | HMAC-signed QR token | P0 | 5 | E5-005 | S5 |
| US-E5-015 | Email PDF e-ticket | P0 | 5 | E5-014, E7-002 | S6 |
| **Epic E5 total** | | | **60** | | |

---

## Epic E6 — Check-in & Validation

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E6-001 | Organizer creates check-in staff | P0 | 5 | E2-003 | S6 |
| US-E6-002 | Browser scanner login + UI | P0 | 5 | E6-001 | S6 |
| US-E6-003 | Scan outcomes (7 cases) | P0 | 8 | E6-002, E5-014 | S6 |
| US-E6-004 | Manual lookup | P0 | 3 | E6-003 | S6 |
| US-E6-005 | Single-entry policy enforcement | P0 | 2 | E6-003 | S6 |
| US-E6-006 | Scan audit log | P0 | 3 | E6-003 | S6 |
| **Epic E6 total** | | | **26** | | |

---

## Epic E7 — Notifications & Reminders

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E7-001 | Verification email | P0 | 2 | — | S1 |
| US-E7-002 | Booking confirmation email + PDF | P0 | 5 | E5-005, E5-014 | S6 |
| US-E7-003 | 24h reminder (2h = P2) | P0 / P2 | 3 | E3-011 | S6 / V1.1 |
| US-E7-004 | Event modification email | P0 | 2 | E3-007 | S6 |
| US-E7-005 | Post-event feedback email | P0 | 2 | E3-011 | S6 |
| US-E7-006 | Organizer lifecycle emails | P0 | 3 | E2-002 | S2 |
| US-E7-007 | Admin alerts (pending org, SMTP fails) | P0 | 2 | E2-002, E7-008 | S2/S6 |
| US-E7-008 | SMTP retry (3x backoff) | P0 | 5 | E7-001 | S1 |
| **Epic E7 total** | | | **24** | | |

---

## Epic E8 — Reviews & Feedback

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E8-001 | Submit review (checked-in only) | P0 | 5 | E6-003 | S7 |
| US-E8-002 | Aggregate event rating display | P0 | 2 | E8-001 | S7 |
| US-E8-002b | Aggregate organizer rating | P2 | 2 | E8-001 | V1.1 |
| US-E8-003 | Admin remove abusive review | P0 | 2 | E8-001 | S7 |
| **Epic E8 total** | | | **11** | | |

---

## Epic E9 — Reporting & Dashboards

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E9-001 | Super Admin dashboard KPIs | P0 | 5 | many | S7 |
| US-E9-002 | Organizer dashboard KPIs | P0 | 5 | many | S7 |
| US-E9-003 | Attendee dashboard KPIs | P0 | 3 | E5-008 | S5 |
| US-E9-004 | CSV exports | P0 | 5 | many | S7 |
| US-E9-005 | KPI cards + tables (charts = P2) | P0 / P2 | 3 | E9-001 | S7 / V1.1 |
| **Epic E9 total** | | | **21** | | |

---

## Epic E10 — Platform, Compliance & Ops

| ID | Story | Pri | SP | Dep | Sprint |
|----|-------|-----|----|----|--------|
| US-E10-001 | Privacy + T&C static pages | P0 | 2 | — | S8 |
| US-E10-002 | Cookie banner | P0 | 1 | — | S8 |
| US-E10-003 | /health endpoint | P0 | 2 | — | S1 |
| US-E10-004 | i18n keys throughout | P0 | 5 | — | S1+ (cross-cutting) |
| US-E10-005 | IST + INR formatting | P0 | 3 | — | S1 |
| US-E10-006 | Structured JSON logs | P0 | 5 | — | S1+ (cross-cutting) |
| US-E10-007 | Daily DB backup | P0 | 3 | — | S8 |
| **Epic E10 total** | | | **21** | | |

---

## Backlog Summary

| Epic | Stories | Total SP | MVP SP | V1.1+ SP |
|------|---------|----------|--------|----------|
| E1 Identity | 12 | 36 | 31 | 5 |
| E2 Organizer | 10 | 29 | 19 | 10 |
| E3 Events | 13 | 46 | 41 | 5 |
| E4 Discovery | 7 | 21 | 21 | 0 |
| E5 Booking & Coupons | 15 | 60 | 54 | 6 |
| E6 Check-in | 6 | 26 | 26 | 0 |
| E7 Notifications | 8 | 24 | 21 | 3 |
| E8 Reviews | 4 | 11 | 9 | 2 |
| E9 Reporting | 5 | 21 | 18 | 3 |
| E10 Platform | 7 | 21 | 21 | 0 |
| **Total** | **87** | **295** | **261** | **34** |

---

## MVP Sizing Sanity Check

- **261 MVP story points** at **25 SP/sprint** = **~10.4 sprints** with conservative team
- With minor scope trim (PM-01 cuts) + sharing of cross-cutting work: **8 sprints feasible**
- Add 1 hardening sprint pre-GA: **9 sprints (~18 weeks / ~4.5 months)** to GA from coding kickoff

This aligns with the V1.0 GA target in PM-02.

---

**End of Backlog**
