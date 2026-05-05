# PM-02 — Product Roadmap

**Project:** Smart Event Management & Ticket Booking Platform
**Owner:** John (PM)
**Date:** 2026-05-05
**Horizon:** ~12 months across V1 → V1.x → V2

---

## 1. Strategic Themes

| Theme | What it means | When |
|-------|---------------|------|
| **T1. Operational Foundation** | A working, trustworthy event-lifecycle + ticketing platform | V1 (MVP) |
| **T2. Polish & Conversion** | Reduce friction, improve attendee/organizer NPS | V1.1–V1.2 |
| **T3. Monetization Readiness** | Real money moves on-platform | V2 |
| **T4. Scale & Reach** | Multi-channel, multi-language, mobile-first | V2.5–V3 |
| **T5. Enterprise & White-Label** | Bigger organizers, branded portals | V3+ |

---

## 2. Roadmap (Now / Next / Later)

```
NOW (V1 — MVP)              NEXT (V1.1–V1.2)             LATER (V2+)
────────────────             ──────────────────           ─────────────────
Identity & Auth              Resubmission flow            Online payment gateway
Organizer onboarding         Sensitive-field re-review    Automated refunds
Event lifecycle              Multi-gallery + 2h reminder  SMS notifications
Ticket inventory             Private event access code    Push notifications
Multi-cat booking            Coupon analytics             Native mobile apps
HMAC QR + check-in           Charts on dashboards         Recurring events
SMTP emails                  ICS calendar download        Waitlist
Coupons (basic)              Aggregate org ratings        Invite-only events
Reviews                      Bulk attendee CSV (offline   Offline check-in scanner
Dashboards (KPI + CSV)        check-in fallback)          Seat maps
Audit + backups              Daily organizer email digest White-label portals
                             Email template editor        Subscription tiers
                                                          Commission engine
                                                          Premium promotion slots
                                                          Multi-language UI
                                                          Multi-country / multi-currency
                                                          RBAC sub-roles
                                                          Google / Apple SSO
```

---

## 3. Release Theming

### V1.0 — "Ship the Loop" (MVP)
Target: ~12 weeks (6 sprints × 2 weeks) post architecture sign-off.
Goal: First 5 real organizers running real events on the platform with successful QR check-ins.

### V1.1 — "Trust & Polish" (~6–8 weeks after V1)
Driver: Feedback from first 30 organizers. Closes obvious UX gaps.
Includes: Resubmission flow, sensitive-field re-review, 2h reminders, ICS download, gallery images, private access codes, charts.

### V1.2 — "Operational Resilience" (~6–8 weeks after V1.1)
Driver: Operational pain points surfacing at ~50 organizers.
Includes: Bulk attendee CSV (offline-day fallback), daily organizer digest, coupon analytics, email template editor, aggregate organizer ratings.

### V2.0 — "Money Moves" (~3 months after V1.2)
**The big one.** Payment gateway integration (Razorpay/Stripe), automated refund workflow, payment-status booking lifecycle, GST invoicing.
Also: SMS notifications (Brevo/MSG91), push (Firebase free), basic monetization (commission %).

### V2.5 — "Scale" (~3 months after V2)
Native mobile app (Flutter/React Native, OR PWA polish), recurring events, waitlist, advanced seat maps (start with simple grid), Google SSO, organizer tiers.

### V3.0 — "Enterprise"
White-label organizer subdomains, multi-language UI (Hindi, Tamil, Telugu first), RBAC sub-roles, premium promotions, multi-country (start with neighboring SAARC).

---

## 4. Quarterly View (rough timeline)

| Quarter | Focus | Releases |
|---------|-------|----------|
| **Q1 / Q2 (current)** | Build & ship MVP | V1.0 GA |
| **Q3** | Polish + Resilience | V1.1, V1.2 |
| **Q4** | Monetization | V2.0 |
| **Q1 next year** | Scale | V2.5 |
| **Q2 next year** | Enterprise | V3.0 (early) |

(Calibrate to actual team velocity from Sprint 1–3 measurements.)

---

## 5. Roadmap Dependencies

```
V1 Identity ──► V1 Organizer ──► V1 Events ──► V1 Booking ──► V1 QR/Check-in
                                                         │
                                                         ▼
                                                    V2 Payment ──► V2 Refunds ──► V2 GST Invoice
                                                                        │
V1 Notification (SMTP) ────────────────────────────────────────────────┴──► V2 SMS/Push channel abstraction
V1 Storage (local) ────────────────────────────────────────────────────────► V2 Cloud storage swap
V1 i18n keys ──────────────────────────────────────────────────────────────► V3 Hindi/Tamil
V1 Single Admin Role ──────────────────────────────────────────────────────► V3 RBAC sub-roles
```

---

## 6. North-Star Metric

**Active Validated Tickets per Month** = `count(tickets where status = CheckedIn)` per calendar month.

Why: Combines acquisition (organizer onboarding), conversion (booking), and value delivery (actual attendance). One number that goes up if everything is healthy.

**Target ladder:**
- M1: 100
- M3: 1,000
- M6: 5,000
- M12 (V2 live): 25,000

---

**End of Roadmap**
