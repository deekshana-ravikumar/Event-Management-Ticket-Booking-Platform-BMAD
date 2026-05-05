# 02 — Enterprise Business Requirements Document (BRD)

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0
**Date:** 2026-05-05
**Author:** Mary — Business Analyst
**Status:** Approved

---

## 1. Executive Summary

The Smart Event Management & Ticket Booking Platform is a centralized digital system designed to streamline event hosting and ticket booking for the Indian market. It enables event organizers to digitally publish, manage, and operate events while providing attendees a frictionless way to discover events, book tickets, receive secure QR-based e-tickets, and check in at venues.

V1 deliberately defers monetization (payments, subscriptions, premium promotions) to focus on operational completeness, low-cost open-source delivery, and a payment-ready architecture for Phase 2 expansion.

---

## 2. Business Context

### 2.1 Problem Statement

Small and mid-sized event organizers in India today often rely on:
- Manual spreadsheets for attendee registration
- WhatsApp / phone-call-based ticket confirmations
- Paper tickets prone to loss, duplication, and forgery
- Fragmented tools (Google Forms + Excel + manual emails) with no unified attendee or check-in record

This creates operational chaos at the venue (slow entry, disputes, no real-time attendance visibility) and a poor attendee experience.

### 2.2 Opportunity

A unified, low-cost, organizer-friendly platform — built on free/open-source technology — can:
- Replace fragmented manual workflows with a single digital lifecycle
- Provide secure, tamper-resistant QR ticketing
- Give organizers real-time attendance + analytics dashboards
- Build the operational backbone for monetization in Phase 2

### 2.3 Competitive Landscape (Reference)

| Platform | Strength | V1 Differentiation |
|----------|----------|---------------------|
| BookMyShow | Brand, scale, payment integration | Smaller organizer focus, no high commission |
| Eventbrite | Self-service organizer flow | India-localized, INR-only, low-cost ops |
| Townscript | India-focused | Centralized admin oversight + simplified workflows |

**V1 differentiators:**
1. Simplified organizer event management
2. Centralized admin monitoring with approval queue
3. Secure QR-based digital ticket validation
4. Easy attendee booking workflow
5. Low-cost open-source implementation
6. Future enterprise-readiness (payment, monetization, white-label hooks)

---

## 3. Business Objectives & Goals

| Goal | Metric | V1 Target (6 months) |
|------|--------|----------------------|
| Onboard organizers | # active organizers | 100+ |
| Enable event hosting | # events published | 500+ / month |
| Acquire attendees | # registered attendees | 10,000+ |
| Deliver bookings | # confirmed bookings | Tracked monthly |
| Validate attendance | # successful QR check-ins | Tracked per event |
| Operate reliably | Platform uptime | ≥ 99% |

---

## 4. Stakeholder Analysis

| Stakeholder | Role | Influence | Interest | Key Needs |
|-------------|------|-----------|----------|-----------|
| Super Admin (platform owner) | Govern platform | High | High | Approve organizers, monitor health, intervene in disputes |
| Event Organizer | Tenant | High | High | Easy event setup, attendee visibility, reporting |
| Attendee | Customer | Medium | High | Discover events, book quickly, smooth entry |
| Check-in Staff | Operator | Low | Medium | Fast scanning, reliable validation, manual fallback |
| Development Team | Implementer | High | High | Clear requirements, free/open-source stack |
| Compliance / Legal | Reviewer | Low | Medium | Secure data handling, T&C, privacy policy |

---

## 5. Scope Statement

### In Scope (V1)
End-to-end event lifecycle: organizer onboarding, event creation, ticket inventory, attendee booking, QR ticketing, check-in validation, coupons, email notifications, reviews, and dashboards. India-only, INR-only, English-only.

### Out of Scope (V1)
Payments, refunds, SMS / push notifications, seat maps, recurring events, white-labeling, mobile apps, multi-country, RBAC sub-roles. (Detailed list: see Document 01 §4.)

---

## 6. Business Capabilities (V1)

| # | Capability | Owner Module |
|---|------------|--------------|
| BC-01 | User registration, authentication, profile management | Identity |
| BC-02 | Organizer onboarding & admin approval workflow | Organizer Mgmt |
| BC-03 | Event creation with venue / date-time scheduling | Event Mgmt |
| BC-04 | Ticket categories, pricing, quantity & sale-window mgmt | Ticket Inventory |
| BC-05 | Event publishing & lifecycle management | Event Lifecycle |
| BC-06 | Public event discovery, search & filtering | Discovery |
| BC-07 | Attendee booking workflow (free + reserved/pay-later) | Booking |
| BC-08 | Coupon configuration & application | Coupon |
| BC-09 | Secure QR/e-ticket generation & delivery | Ticketing |
| BC-10 | On-site QR-based check-in validation | Check-in |
| BC-11 | Booking cancellation (attendee / organizer / admin) | Booking |
| BC-12 | Email notifications (transactional, lifecycle, reminders) | Notification |
| BC-13 | Review & feedback collection | Reviews |
| BC-14 | Dashboards & reporting (super admin / organizer / attendee) | Reporting |
| BC-15 | Audit logging & operational observability | Platform Ops |
| BC-16 | Static legal pages & cookie notice | Platform |

---

## 7. High-Level Business Process Flows

### 7.1 Organizer Onboarding
1. Organizer signs up with profile + (optional) documents
2. System sends email verification
3. Organizer verifies email → status = Pending Admin Approval
4. Super Admin reviews queue
5. Outcome: Approve / Reject / Request Resubmission
6. Organizer notified by email
7. Approved organizers can create + publish events

### 7.2 Event Lifecycle
1. Organizer creates event (Draft)
2. Adds ticket categories (qty > 0)
3. Validates pre-publish criteria → Publishes
4. Event visible in discovery
5. Attendees book; inventory atomically decrements
6. Organizer may unpublish (preserves bookings) or cancel (revokes tickets)
7. Scheduler auto-completes event after end + 2-hr grace
8. Post-event: feedback request, ratings aggregated

### 7.3 Attendee Booking & Check-in
1. Attendee browses → selects event → chooses category & qty
2. Applies optional coupon
3. Enters per-attendee names + accepts T&C → Confirms
4. Inventory decremented atomically; booking created (`BK-YYYY-NNNNNN`)
5. QR codes issued (one per ticket); PDF e-ticket emailed
6. On event day: check-in staff scans QR via web scanner
7. System validates token (active / not duplicate / right event) → marks Checked In
8. Audit log records every scan

---

## 8. Business Rules (Summary)

(Full catalog in Document 08.)

- Attendee booking requires login.
- Inventory decrement must be atomic; zero overselling.
- Self-cancellation only ≥ 24 hours before event start; returns inventory; revokes QR.
- One coupon per booking; cannot be applied to free bookings.
- Single-entry policy: one QR scan = one entry; second scan rejected.
- Total capacity cannot be reduced below already-booked count.
- Organizer suspension preserves attendee bookings unless event explicitly cancelled.
- Reviews allowed only after successful check-in.

---

## 9. Assumptions

- A1. Stakeholder will provide final Privacy Policy + T&C content before launch.
- A2. SMTP credentials (e.g., Gmail SMTP, Sendinblue free tier, Brevo) will be supplied.
- A3. No payment / refund obligations in V1; events are free or pay-at-venue.
- A4. Organizers operate in good faith; admin can intervene reactively.
- A5. Attendees have email access for verification + ticket delivery.
- A6. Venues have basic Wi-Fi for check-in scanner connectivity.

---

## 10. Constraints

- C1. Free / open-source stack mandatory.
- C2. Limited startup budget; low-cost VPS hosting.
- C3. India / INR / English / IST only in V1.
- C4. No paid third-party APIs.
- C5. Single Super Admin; no RBAC sub-roles in V1.
- C6. Locked tech stack: Angular + ASP.NET Core + MySQL + JWT + SMTP.

---

## 11. Dependencies

- D1. SMTP service availability and deliverability.
- D2. SSL certificate (Let's Encrypt) for HTTPS.
- D3. VPS provisioning + Docker runtime.
- D4. Domain name procurement.
- D5. Privacy Policy / T&C content from legal.

---

## 12. Success Criteria & KPIs

| KPI | Target (6 months) |
|-----|-------------------|
| Organizer onboarding count | 100+ |
| Events published | 500+ / month |
| Registered attendees | 10,000+ |
| Booking transactions | Tracked, growth trend positive |
| Successful QR check-ins | ≥ 90% of confirmed tickets |
| Platform uptime | ≥ 99% |
| Email delivery success | ≥ 98% (after 3 retries) |
| Critical security incidents | 0 |

---

## 13. Risks (Business)

| ID | Risk | Likelihood | Impact | Mitigation |
|----|------|-----------|--------|-----------|
| BR1 | Organizers reject "no payments" V1 | Medium | High | Position as pilot; fast-follow Phase 2 payments |
| BR2 | Attendee mistrust without paid platforms' brand | Medium | Medium | Verified Organizer badge + secure QR |
| BR3 | SMTP throttling / spam-filter bouncing | Medium | Medium | Use reputable SMTP; SPF/DKIM/DMARC setup |
| BR4 | QR fraud / sharing screenshots | Low | High | HMAC-signed tokens + single-entry policy + staff audit |
| BR5 | Server overload during popular event | Low | Medium | DB locking + capacity planning + scalable VPS |
| BR6 | Data loss / breach | Low | High | Daily backups, HTTPS, OWASP baseline, encrypted disk |

---

## 14. Glossary

| Term | Definition |
|------|------------|
| **Organizer** | Approved tenant who creates and operates events |
| **Attendee** | Registered end-user who books tickets |
| **Check-in Staff** | Organizer-created operator who validates tickets at the venue |
| **Booking** | A confirmed transaction containing 1+ tickets |
| **Ticket** | An individual entry pass; one QR per ticket |
| **QR Token** | HMAC-signed opaque token encoding ticket identity |
| **Reserved / Pay-Later** | V1 booking mode that captures price intent without collecting money |
| **Sold Out** | Derived flag when total ticket inventory across all categories = 0 |
| **Verified Organizer** | Public badge on approved organizer profile |
| **IST** | Indian Standard Time (Asia/Kolkata, UTC+05:30) |

---

**End of BRD**
