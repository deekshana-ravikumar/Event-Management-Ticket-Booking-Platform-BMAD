# PM-01 — MVP Boundary & Feature Prioritization

**Project:** Smart Event Management & Ticket Booking Platform
**Owner:** John (PM)
**Date:** 2026-05-05

---

## 1. MVP Definition (Product Manager's Lens)

**MVP Hypothesis:**
> *Small-to-mid Indian event organizers will adopt a free, centralized digital platform to publish events, sell tickets (or register attendees), and validate entry via QR — even without integrated online payment — IF the workflow is materially faster than their current spreadsheet+WhatsApp+manual-ticket setup.*

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
| Organizer onboarding + Super Admin approval | Quality gate; trust signal |
| Event creation + publishing | Core organizer value |
| Ticket categories + pricing + inventory | Required to issue tickets |
| Public discovery + search + filter | Required for attendee acquisition |
| Logged-in attendee booking (multi-category, multi-attendee) | Core attendee value |
| Atomic inventory decrement (zero overselling) | Trust-critical |
| QR ticket generation (HMAC-signed, one per ticket) | THE differentiator |
| Browser-based QR check-in + manual lookup | Validates the loop |
| SMTP transactional emails (verification, booking confirmation, cancellation, event change, reminders) | Operational lifeline |
| Booking self-cancellation (≥24h) | Standard expectation |
| Coupon engine (organizer + global) | Required for promotions |
| Reviews (post-checkin) | Quality signal |
| Super Admin / Organizer / Attendee dashboards (basic KPIs + CSV exports) | Operational visibility |
| Audit log (organizer state, scan, admin actions) | Compliance + dispute resolution |
| Privacy + T&C + cookie banner | Legal minimum |
| Health endpoint + structured logs + daily backups | Operational baseline |

### SHOULD HAVE (MVP if time, else V1.1)
High value, but MVP can launch without — workarounds exist.

| Capability | Defer rationale |
|------------|-----------------|
| Resubmission Pending workflow (third approval outcome) | Approve/Reject suffices for first 50 organizers; "request more info" can be done out-of-band via email |
| Sensitive-field re-review on profile edit | Lock all profile edits during V1 if needed; relax in V1.1 |
| Aggregate ratings on organizer profile | Event-level rating is the priority; org-level is derived |
| ICS calendar download | Nice-to-have polish |
| Charts on dashboards | KPI cards + tables suffice for first 10–20 organizers |
| Staff event-assignment UX (vs simple "any of my events") | Simpler model works at low scale |

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

- Online payment gateway · refunds · SMS · push · seat maps · monetization plans · white-label · native apps · SSO · recurring events · waitlist · invite-only CSV · offline check-in · geo maps · multi-language UI · multi-country · RBAC sub-roles · custom organizer questions · advanced GST invoicing

---

## 3. PM Challenges to BA Scope (and Decisions)

I'm not here to rubber-stamp. Three places I push back:

### Challenge #1 — "Reserved / Pay Later" booking mode
**My concern:** Indians don't reserve concert tickets without paying. This mode might generate phantom bookings and inventory hoarding.
**Decision:** Keep it (BA locked) BUT enforce two PM safeguards:
- Show a clear "Reserved — Pay at Venue" badge on the attendee booking & ticket
- Track **no-show rate per organizer**; if > 40%, surface to admin in dashboard
- Add a **per-attendee active-reservation cap** (e.g., max 3 unpaid reservations across all events) to limit hoarding — *new business rule for the PRD*

### Challenge #2 — "100+ organizers in 6 months"
**My concern:** With self-serve signup + manual admin approval + no marketing budget, 100 is aggressive.
**Decision:** Split into measurable sub-targets in the success metric — *50 onboarded by month 3, 100 by month 6* — and add a "seed organizer" outreach plan to the launch playbook.

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

## 5. MVP Cut-Line Decisions

| Capability | In MVP? | Rationale |
|------------|---------|-----------|
| Resubmission Pending state | ❌ → V1.1 | Approve/Reject covers 80% |
| Sensitive-field re-review | ❌ → V1.1 | Out-of-band admin handling acceptable at low scale |
| 2-hour reminder email | ❌ → V1.1 | 24h is enough |
| ICS calendar download | ❌ → V1.1 | Polish, not value |
| Private event access code | ❌ → V1.1 | Unlisted URL covers it |
| Multi-gallery images (3) | ❌ → V1.1 | Banner sufficient |
| Coupon redemption dashboard | ❌ → V1.1 | CSV export sufficient |
| Charts on dashboards | ❌ → V1.1 | KPI cards + tables OK |
| Aggregate organizer rating | ❌ → V1.1 | Event ratings only at MVP |

**Net effect:** ~10–15 stories trimmed from MVP, accelerating launch by ~1 sprint.

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
