# PM-01 — MVP Boundary & Feature Prioritization

**Project:** Smart Event Management & Ticket Booking Platform
**Owner:** John (PM)
**Date:** 2026-05-05 · **Revised:** 2026-05-06 (BA validation + V1 booking model lock)

---

## 0. V1 Booking Model — LOCKED (Mandatory)

🔴 **Final, non-negotiable for V1:**

- **No payment gateway, no payment intent, no payment status, no refund logic, no transaction ledger.**
- **No "Reserved", "Pending Payment", "Pay Later", or "Hold" state.** Booking has exactly two terminal states: `Confirmed` or `Cancelled`.
- **All bookings are instant confirmation.** Inventory decrements atomically the moment the user clicks Confirm.
- **Ticket price is informational only** (free-readiness for V2). UI displays ₹ amount; no money is exchanged inside the platform.
- **QR ticket generated immediately** on confirm; confirmation page rendered; confirmation email + PDF dispatched async.
- **Cancellation** (≥24 h before event) restores inventory; no refund computation occurs.

**Final V1 booking flow** (canonical — referenced by all downstream artifacts):
1. Attendee selects event
2. Selects ticket category(ies) + quantity (subject to per-attendee per-event cap, see BR-NEW-01)
3. Optionally applies one coupon
4. Provides per-ticket attendee names + accepts T&C
5. Clicks **Confirm Booking**
6. System atomically: validates inventory + cap → decrements inventory → creates Booking + Tickets → generates HMAC QR per ticket → records consent → returns `BK-YYYY-NNNNNN`
7. Confirmation page shown with downloadable QRs
8. Async: confirmation email with PDF e-ticket sent

**BR-NEW-01 (replaces phantom "reservation cap"):** Per attendee, per event, **max 10 tickets total across all categories** in V1. Enforced inside the atomic confirm transaction. Configurable per event by organizer in V1.1.

---

## 1. MVP Definition (Product Manager's Lens)

**MVP Hypothesis:**
> *Small-to-mid Indian event organizers will adopt a free, centralized digital platform to publish events, register attendees, and validate entry via QR — even without integrated online payment — IF the workflow is materially faster than their current spreadsheet+WhatsApp+manual-ticket setup.*

**MVP succeeds if (6 months):**
- ≥ 100 organizers approved AND ≥ 30 of them publish ≥ 1 event AND repeat (publish a 2nd event)
- ≥ 500 events published
- ≥ 70% of issued tickets get a successful QR check-in scan (validates the core value)
- ≥ 99% uptime
- Zero booking-data loss / overselling incidents

If we miss the **repeat-publish** number, the value prop is broken — re-evaluate before Phase 2 build-out.

---

## 2. MoSCoW Classification

### MUST HAVE (MVP / V1)
The minimum coherent product. Removing any of these breaks the core value loop.

| Capability | Why it's Must |
|------------|---------------|
| User registration + login + email verification (Attendee, Organizer) | Identity is foundational |
| Organizer onboarding + Super Admin approval (Approve / Reject / **Request Resubmission**) | Quality gate; trust signal; resubmission unblocks borderline orgs |
| **Organizer profile edit — non-sensitive fields** (logo, description, contact phone, address) | Otherwise every typo becomes a support ticket |
| **Organizer profile edit — sensitive fields** (Org Name, PAN, GSTIN, Business Reg) → admin re-approval workflow | Identity-drift / fraud prevention |
| Event creation + publishing | Core organizer value |
| Ticket categories + **informational pricing** + inventory (no payment logic) | Required to issue tickets; price is display-only in V1 |
| Public discovery + search + filter | Required for attendee acquisition |
| Logged-in attendee booking (multi-category, multi-attendee) — **instant confirmation only** | Core attendee value |
| Atomic inventory decrement + **per-attendee per-event ticket cap** (zero overselling, anti-hoarding) | Trust-critical |
| QR ticket generation (HMAC-signed, one per ticket, generated on Confirm) | THE differentiator |
| Browser-based QR check-in + manual lookup | Validates the loop |
| SMTP transactional emails (verification, booking confirmation + PDF, cancellation, event change, reminders, organizer lifecycle) | Operational lifeline |
| Booking self-cancellation (≥24 h, restores inventory, no refund computation) | Standard expectation |
| Coupon engine (organizer + global) + **basic redemption stats** (count + revenue impact) | Required for promotions; stats needed by organizer dashboard |
| Reviews (post check-in only) | Quality signal |
| Super Admin / Organizer / Attendee dashboards (KPIs + CSV exports) | Operational visibility |
| Audit log (organizer state, scan, admin actions, **booking + cancel events**) | Compliance + dispute resolution |
| **DPDP consent ledger** (T&C version, IP, UA, timestamp on signup + on each booking) | Indian data-protection compliance baseline |
| Privacy + T&C + cookie banner | Legal minimum |
| Health endpoint + structured logs + daily backups | Operational baseline |

### SHOULD HAVE (MVP if time, else V1.1)
High value, but MVP can launch without — workarounds exist.

| Capability | Defer rationale |
|------------|-----------------|
| Aggregate ratings on organizer profile | Event-level rating is the priority; org-level is derived |
| ICS calendar download | Nice-to-have polish |
| Charts on dashboards | KPI cards + tables suffice for first 10–20 organizers |
| Staff event-assignment UX (vs simple "any of my events") | Simpler model works at low scale |
| Coupon analytics drill-down (per-coupon time series, attendee segmentation) | Basic count + revenue stats ship in MVP; deeper analytics V1.1 |
| Attendee data export (DPDP "download my data") | Legal expectation but enforceable on request in V1; self-serve in V1.1 |
| Account deletion with anonymized retention (DPDP "erasure") | Deactivation suffices for V1; full erasure flow V1.1 |

### COULD HAVE (V1.x)
Worthwhile but explicitly post-MVP.

| Capability | Notes |
|------------|-------|
| Private event access codes | Unlisted URL is enough at V1 |
| Email reminder 2-hour (alongside 24-hour) | One reminder usually sufficient |
| Coupon redemption analytics dashboard | Counts in CSV is the V1 floor |
| PDF event report export | CSV is enough |
| Multiple gallery images | One banner is enough at V1 |
| Multi-day events (start/end range UX polish) | Underlying schema supports it; UX polish optional |

### WON'T HAVE (V1) — explicitly deferred
Listed in Mary's scope baseline — confirming PM agrees:

- **Online payment gateway · payment intent · payment status · refunds · transaction ledger** · SMS · push · seat maps · monetization plans · white-label · native apps · SSO · recurring events · waitlist · invite-only CSV · offline check-in · geo maps · multi-language UI · multi-country · RBAC sub-roles · custom organizer questions · advanced GST invoicing · **reserved / pay-later / hold-then-pay flows of any kind**

---

## 3. PM Challenges to BA Scope (and Decisions)

I'm not here to rubber-stamp. Three places I push back:

### Challenge #1 — Anti-hoarding under instant-confirm + free pricing
**My concern:** With informational pricing + instant confirm + no payment friction, a single attendee can grab 50 tickets to a popular free event in seconds, blocking real attendees.
**Decision (locked with BA):** Enforce **BR-NEW-01** — per attendee, per event, max 10 tickets across all categories in V1. Validated atomically inside US-E5-005 confirm transaction. Configurable per organizer in V1.1. *No "reservation" / "hold" state — confirmed bookings only, capped by count.*

### Challenge #2 — "100+ organizers in 6 months"
**My concern:** With self-serve signup + manual admin approval + no marketing budget, 100 is aggressive.
**Decision:** Split into measurable sub-targets — *50 onboarded by month 3, 100 by month 6* — and add a "seed organizer" outreach plan to the launch playbook.

### Challenge #3 — Browser-only check-in scanner
**My concern:** Venue WiFi reliability is a real risk in India. We accepted "online only" for V1, but I want a graceful failure mode.
**Decision:** Add **bulk attendee CSV download** (per event, day-of) as a Should-Have so staff can validate manually if connectivity dies. This is small to build and dramatically reduces operational risk.

---

## 4. RICE-Lite Prioritization (Top 15)

Scoring: **R**each (1-5), **I**mpact (1-5), **C**onfidence (%), **E**ffort (story points). Score = R×I×Conf / E.

| Rank | Feature | R | I | Conf | E | Score | Notes |
|------|---------|---|---|------|---|-------|-------|
| 1 | Login / Auth foundation | 5 | 5 | 100% | 8 | 3.13 | Blocks everything |
| 2 | Event create + publish | 5 | 5 | 95% | 13 | 1.83 | Core organizer value |
| 3 | Atomic booking + inventory | 5 | 5 | 95% | 13 | 1.83 | Core attendee value, trust-critical |
| 4 | QR generate + scan + audit | 5 | 5 | 90% | 13 | 1.73 | THE differentiator |
| 5 | Public discovery + search | 5 | 4 | 95% | 8 | 2.38 | Without this, no traffic |
| 6 | Organizer approval workflow | 4 | 4 | 100% | 5 | 3.20 | Trust gate |
| 7 | SMTP + booking confirmation email | 5 | 5 | 95% | 5 | 4.75 | Operational lifeline; cheap |
| 8 | Attendee dashboard (bookings) | 5 | 4 | 95% | 5 | 3.80 | Re-download QR is critical |
| 9 | Booking cancellation | 4 | 3 | 95% | 3 | 3.80 | Standard expectation |
| 10 | Organizer dashboard + CSV exports | 4 | 4 | 90% | 8 | 1.80 | Differentiator vs spreadsheet world |
| 11 | Admin dashboard + queue | 3 | 4 | 95% | 5 | 2.28 | Internal tool — depth matters less |
| 12 | Coupon engine | 3 | 3 | 85% | 8 | 0.96 | Useful but not blocking |
| 13 | Reviews | 4 | 2 | 80% | 5 | 1.28 | Quality signal; can launch w/o |
| 14 | Reminders (24h) | 5 | 3 | 90% | 3 | 4.50 | Cheap, high-leverage |
| 15 | Audit logs (cross-cutting) | 3 | 4 | 100% | 5 | 2.40 | Foundational; build with each module |

**Top 4 must form Sprint 1–4. Coupons & Reviews can land in Sprint 5–6.**

---

## 5. MVP Cut-Line Decisions (Revised after BA validation)

| Capability | In MVP? | Rationale |
|------------|---------|-----------|
| Resubmission Pending state (US-E2-005) | ✅ **Promoted to MVP (P1, S3)** | Required to onboard 100 orgs without rigid Approve/Reject |
| Org profile edit — non-sensitive (US-E1-010a NEW) | ✅ **MVP P0, S2** | Otherwise every typo = DB hand-edit |
| Org profile edit — sensitive + admin re-review (US-E2-007) | ✅ **Promoted to MVP P1, S8** | Identity-drift prevention; user-mandated |
| Coupon redemption stats (US-E5-013) | ✅ **Promoted to MVP P0, S9** | Organizer dashboard depends on it |
| DPDP consent ledger (US-E10-008 NEW) | ✅ **MVP P0, S1** | Indian compliance baseline |
| 2-hour reminder email | ❌ → V1.1 | 24h is enough |
| ICS calendar download | ❌ → V1.1 | Polish, not value |
| Private event access code | ❌ → V1.1 | Unlisted URL covers it |
| Multi-gallery images (3) | ❌ → V1.1 | Banner sufficient |
| Coupon analytics drill-down | ❌ → V1.1 | Basic count/revenue ships in MVP |
| Charts on dashboards | ❌ → V1.1 | KPI cards + tables OK |
| Aggregate organizer rating | ❌ → V1.1 | Event ratings only at MVP |
| Attendee data export (US-E10-009 NEW) | ❌ → V1.1 | On-request via admin in V1 |
| Account deletion w/ anonymization (US-E10-010 NEW) | ❌ → V1.1 | Deactivation (US-E1-011) suffices for V1 |

**Net effect:** +5 stories promoted into MVP (org edit split, resubmission, sensitive re-review, coupon stats, consent ledger). MVP grows by ~17 SP but eliminates 5 critical gaps. New MVP total = ~278 SP across **9 sprints**.

---

## 6. MVP "Definition of Done" (Product-Level)

MVP is shippable to first paying organizers when:

- ✅ All MUST HAVE capabilities pass acceptance criteria from Mary's [07-acceptance-criteria.md](../planning-artifacts/07-acceptance-criteria.md)
- ✅ All MUST HAVE business rules from [08-business-rules-edge-cases.md](../planning-artifacts/08-business-rules-edge-cases.md) implemented + tested
- ✅ NFR targets in [09-non-functional-requirements.md](../planning-artifacts/09-non-functional-requirements.md) met for: HTTPS, BCrypt, JWT, atomic inventory, P95 latency, 99% uptime, daily backup
- ✅ At least 3 internal end-to-end UAT runs (organizer create → attendee book → check-in) pass without intervention
- ✅ Privacy + T&C + cookie banner live
- ✅ Production deployment runbook tested in staging
- ✅ At least 5 seed organizers onboarded in staging and at least 1 real event piloted

---

**End of MVP Boundary & Prioritization**
