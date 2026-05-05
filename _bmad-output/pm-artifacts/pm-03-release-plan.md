# PM-03 — Release Plan

**Project:** Smart Event Management & Ticket Booking Platform
**Owner:** John (PM)
**Date:** 2026-05-05
**Scope:** V1.0 GA release plan with gates

---

## 1. Release Gates Overview

```
┌─────────────┐   ┌──────────────┐   ┌──────────────┐   ┌──────────┐   ┌─────────────┐
│  Internal   │──▶│   Closed     │──▶│   Open Beta  │──▶│   GA     │──▶│ Hyper-care  │
│   Alpha     │   │    Beta      │   │   (limited)  │   │ (V1.0.0) │   │  (2 weeks)  │
└─────────────┘   └──────────────┘   └──────────────┘   └──────────┘   └─────────────┘
   Sprint 6           Sprint 7           Sprint 8         Post-S8        Post-GA
```

---

## 2. Release Gate Definitions

### Gate A — Internal Alpha (end of Sprint 6)
**Audience:** Dev team + PM + 1 friendly organizer.
**Success criteria:**
- ✅ All MUST HAVE features deployed to staging
- ✅ End-to-end happy path works: organizer signup → admin approval → event publish → attendee book → email delivered → QR scan → check-in confirmed
- ✅ All BR (business rules) from [08](../planning-artifacts/08-business-rules-edge-cases.md) automated-tested for booking, inventory, QR
- ✅ HTTPS, JWT, BCrypt verified
- ✅ Daily backup job runs in staging

**Exit:** PM-led demo to stakeholders. Sign-off → enter Closed Beta.

---

### Gate B — Closed Beta (Sprint 7)
**Audience:** 5 hand-picked seed organizers running 1–2 real low-stakes events each.
**Activities:**
- White-glove onboarding (PM personally walks each organizer through)
- Daily standup on bug reports + UX friction
- Hot-fix turnaround target ≤ 24h
- Capture: time-to-first-event-published, time-to-first-booking, scan failure rate

**Success criteria for exit:**
- ✅ 5 organizers complete a full lifecycle event without dev intervention
- ✅ Zero data loss / overselling / payment-relevant defects
- ✅ All P0 bugs closed, P1 bugs ≤ 5 open
- ✅ Average organizer NPS-lite ≥ 7/10
- ✅ Average attendee booking-flow completion ≥ 85% (start → confirmation)

**Exit:** PM go/no-go. Sign-off → Open Beta.

---

### Gate C — Open Beta (Sprint 8)
**Audience:** Public signups capped at first 30 organizers; attendee signups uncapped.
**Activities:**
- Light marketing (LinkedIn / community posts)
- In-product feedback widget
- Weekly metrics review

**Success criteria for exit:**
- ✅ ≥ 25 of 30 organizers publish an event
- ✅ ≥ 20 events fully completed (publish → attendees check in)
- ✅ Uptime ≥ 99% during beta window
- ✅ No P0 incidents
- ✅ Email deliverability ≥ 98% after 3 retries

**Exit:** Final go/no-go meeting → GA.

---

### Gate D — General Availability (V1.0.0)
**Audience:** Open public; growth limited only by funnel.
**Pre-launch checklist:**
- ✅ Production environment provisioned + load-tested at 500 concurrent users
- ✅ Backup + restore drill performed end-to-end
- ✅ Runbook + on-call rotation defined
- ✅ Privacy Policy + T&C reviewed by legal/stakeholder
- ✅ SMTP sender domain SPF/DKIM/DMARC configured
- ✅ Monitoring (uptime + Sentry) wired
- ✅ /health endpoint pinged externally
- ✅ Marketing landing page live
- ✅ Rollback plan documented

---

### Gate E — Hyper-care (2 weeks post-GA)
- Daily metrics review
- 24h response on any P0/P1
- No new feature work; only bug fixes + small UX
- Exit: 14 consecutive days without P0 → enter normal cadence + start V1.1 backlog refinement

---

## 3. Release Inclusions Summary

| Release | Headline | Approx Stories | Sprints |
|---------|----------|----------------|---------|
| V1.0.0 (GA) | Smart Event Platform launch | ~70 (MVP cut) | 8 (~16 weeks) |
| V1.1.0 | Trust & Polish | ~15 | 2–3 |
| V1.2.0 | Operational Resilience | ~12 | 2–3 |
| V2.0.0 | Money Moves (Payments) | ~25 | 5–6 |
| V2.5.0 | Scale (Mobile, SSO, recurring) | ~30 | 6–8 |
| V3.0.0 | Enterprise (White-label, multi-lang) | ~40 | 8–10 |

---

## 4. Versioning Strategy

- **Semantic Versioning:** `MAJOR.MINOR.PATCH`
- MAJOR = breaking API or paradigm shift (e.g., V2 payment integration)
- MINOR = additive features (V1.1, V1.2)
- PATCH = bug fixes, hyper-care releases (V1.0.1, V1.0.2)
- Tag releases in GitHub; auto-deploy via GitHub Actions on tag

---

## 5. Hot-Fix Policy

- P0 (data loss / security / outage): patch within 4 hours, no PRD process
- P1 (functional break, no workaround): patch within 24 hours
- P2 (UX / minor): batch into next minor release
- All hot-fixes deploy via the same pipeline (no manual SSH); rollback within 5 minutes via tag revert

---

## 6. Communication Plan

| Audience | What | When |
|----------|------|------|
| Stakeholder | Demo + status | End of every sprint |
| Beta organizers | Slack/email channel for Q&A | Daily during beta |
| Public | Status page (statuspage.io free tier) | Always |
| Internal team | #platform-incidents Slack | Real-time on alerts |

---

**End of Release Plan**
