# 03 — Functional Requirements Specification (FRS)

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0
**Date:** 2026-05-05

---

## Conventions

- Each requirement carries an ID `FR-<MOD>-NNN`.
- Modules: `IDN` Identity, `ORG` Organizer, `EVT` Event, `TKT` Ticket Inventory, `DSC` Discovery, `BKG` Booking, `CPN` Coupon, `QRT` QR Ticketing, `CHK` Check-in, `NTF` Notification, `RVW` Review, `RPT` Reporting, `ADM` Admin Console, `PLT` Platform.
- Each requirement is testable and unambiguous.

---

## Module IDN — Identity & Access

| ID | Requirement |
|----|-------------|
| FR-IDN-001 | The system shall allow attendees to self-register with Full Name, Email, Phone, City, and Password. |
| FR-IDN-002 | The system shall allow organizers to self-register with Org Name, Contact Person, Email, Phone, Business Address, Event Category Hosted, Logo, Password, and optional documents (PAN, GSTIN, ID Proof, Business Reg Cert). |
| FR-IDN-003 | The system shall send an SMTP-based email verification link upon registration; the account shall remain in `Pending Verification` until the link is clicked. |
| FR-IDN-004 | The system shall enforce password policy: ≥ 8 chars, ≥ 1 uppercase, ≥ 1 lowercase, ≥ 1 numeric, ≥ 1 special character. |
| FR-IDN-005 | The system shall lock an account for 15 minutes after 5 consecutive failed login attempts. |
| FR-IDN-006 | The system shall provide forgot-password via SMTP email reset link with single-use token (24-hr expiry). |
| FR-IDN-007 | The system shall use Email as the unique login identifier across all roles. |
| FR-IDN-008 | The system shall issue JWT access tokens (30 min idle) + refresh tokens upon successful authentication. |
| FR-IDN-009 | The system shall support multi-device login and a "Remember Me" option. |
| FR-IDN-010 | The system shall preserve transactional history when an attendee deactivates their account. |
| FR-IDN-011 | The system shall allow Super Admin to suspend / reactivate any user account with mandatory reason. |

## Module ORG — Organizer Onboarding & Approval

| ID | Requirement |
|----|-------------|
| FR-ORG-001 | The system shall move organizer to `Pending Admin Approval` after email verification. |
| FR-ORG-002 | The system shall provide Super Admin a "Pending Organizer Approval Queue" sorted by oldest first, displaying Org Name, Reg Date, Contact, Category, Doc Status, Days Pending. |
| FR-ORG-003 | The system shall allow Super Admin to Approve, Reject (with mandatory reason), or Request Resubmission (with comments) for each organizer. |
| FR-ORG-004 | The system shall transition organizer status accordingly (Active / Rejected / Resubmission Pending) and send the corresponding SMTP email. |
| FR-ORG-005 | The system shall allow rejected organizers to reapply with the same email after correction; previous rejection history shall be preserved. |
| FR-ORG-006 | The system shall display a "Verified Organizer" badge on approved organizer public profiles. |
| FR-ORG-007 | The system shall classify organizer profile fields as Free-Edit vs Sensitive; sensitive changes (Org Name, PAN, GSTIN, Business Reg) shall move organizer to `Profile Update Pending Review`. |
| FR-ORG-008 | The system shall allow Super Admin to suspend an organizer with mandatory reason; suspended organizers shall lose dashboard access but their published events remain visible to protect existing bookings. |
| FR-ORG-009 | The system shall allow Super Admin to reactivate a suspended organizer. |
| FR-ORG-010 | The system shall record every organizer state transition (prev → new, actor, reason, timestamp) in an audit log. |

## Module EVT — Event Creation, Scheduling & Lifecycle

| ID | Requirement |
|----|-------------|
| FR-EVT-001 | The system shall allow Active organizers to create events as `Draft`. |
| FR-EVT-002 | The system shall capture mandatory fields: Title, Category, Description (rich text ≤ 3000 chars), Type (Offline/Online/Hybrid), Start, End, Banner, Visibility, Total Capacity. |
| FR-EVT-003 | The system shall require Venue (Name, Address, City, State, Pincode) for Offline/Hybrid events. |
| FR-EVT-004 | The system shall require Online Meeting Link for Online/Hybrid events; the link shall be visible only to confirmed attendees. |
| FR-EVT-005 | The system shall support optional fields: Tagline, Language, Age Guidance, T&C, up to 3 Gallery Images, Website URL, Social Links. |
| FR-EVT-006 | The system shall validate Banner: JPG/PNG/JPEG, ≤ 2 MB. |
| FR-EVT-007 | The system shall enforce: Start ≥ now + 2 hours; End > Start; Start ≤ now + 1 year. |
| FR-EVT-008 | The system shall display all dates in IST. |
| FR-EVT-009 | The system shall allow optional Booking Sale Start / End; defaults: opens on publish, closes at event start. |
| FR-EVT-010 | The system shall allow organizer to manually close bookings early. |
| FR-EVT-011 | The system shall maintain event states: `Draft`, `Published`, `Unpublished`, `Cancelled`, `Completed`, `Closed by Admin`. |
| FR-EVT-012 | The system shall derive a `Sold Out` flag when total available inventory = 0. |
| FR-EVT-013 | The system shall require pre-publish validation (all mandatory fields, ≥ 1 ticket category with qty > 0, banner uploaded, valid future start). |
| FR-EVT-014 | The system shall allow direct organizer publish (no per-event admin approval). |
| FR-EVT-015 | The system shall classify post-publish edits: free (description, banner, gallery, social, T&C); notify-attendees (date/time, venue, online link); restricted (capacity ≥ booked, qty ≥ booked). |
| FR-EVT-016 | Unpublish shall hide event from discovery, block new bookings, and preserve existing bookings. |
| FR-EVT-017 | Cancel shall mark all bookings cancelled, invalidate QR tokens, and notify attendees by email. |
| FR-EVT-018 | A scheduled job shall transition events to `Completed` once `event end_time` passes. |
| FR-EVT-019 | QR check-in validation shall be rejected after `event end_time + 2 hours`. |

## Module TKT — Ticket Inventory

| ID | Requirement |
|----|-------------|
| FR-TKT-001 | The system shall support up to 10 ticket categories per event. |
| FR-TKT-002 | Each category shall capture Name, Price (INR ≥ 0), Qty Available, Min Booking Qty, Max Booking Qty, Sale Start, Sale End, Description. |
| FR-TKT-003 | The system shall support Free Booking and Reserved/Pay-Later booking modes. |
| FR-TKT-004 | The system shall enforce a maximum of 10 tickets per attendee per booking. |
| FR-TKT-005 | The system shall atomically decrement inventory using DB row locking (zero overselling). |
| FR-TKT-006 | The system shall block sale of a category when its available qty = 0. |
| FR-TKT-007 | The system shall return inventory to availability upon attendee cancellation. |

## Module DSC — Discovery & Search

| ID | Requirement |
|----|-------------|
| FR-DSC-001 | The system shall expose Public events on Home, Search, Category, and City pages. |
| FR-DSC-002 | The system shall hide Private (Unlisted) events from discovery; access only via direct URL or optional access code. |
| FR-DSC-003 | The system shall provide search filters: keyword, category, city, date range, free/paid, online/offline/hybrid, language. |
| FR-DSC-004 | The system shall support sort options: soonest upcoming, newest published, A–Z. |
| FR-DSC-005 | Default homepage sort shall be soonest upcoming, then newest published. |

## Module BKG — Booking

| ID | Requirement |
|----|-------------|
| FR-BKG-001 | The system shall require attendee login to book; guest booking is not supported. |
| FR-BKG-002 | The system shall allow multi-category booking in a single transaction (cart). |
| FR-BKG-003 | The system shall capture per-attendee Name for each ticket in a multi-ticket booking. |
| FR-BKG-004 | The system shall require T&C acceptance before booking confirmation. |
| FR-BKG-005 | The system shall confirm booking atomically (no cart timeout) and decrement inventory in the same transaction. |
| FR-BKG-006 | Booking reference shall follow format `BK-YYYY-NNNNNN`. |
| FR-BKG-007 | Booking statuses: `Confirmed`, `Cancelled by Attendee`, `Cancelled by Organizer`, `Cancelled by Admin`, `Completed`, `Expired`. |
| FR-BKG-008 | Ticket statuses: `Issued`, `Checked In`, `Cancelled`, `Expired`. |
| FR-BKG-009 | The system shall display a confirmation page with reference, event summary, ticket summary, downloadable QR PDF, ICS calendar download, email-sent confirmation. |
| FR-BKG-010 | The system shall provide attendee dashboard tabs: Upcoming, Past, Cancelled bookings; with QR re-download. |
| FR-BKG-011 | The system shall allow attendee self-cancellation up to 24 hours before event start. |
| FR-BKG-012 | Cancellation shall mark booking & tickets cancelled, invalidate QR tokens, return inventory, and send email. |

## Module CPN — Coupons

| ID | Requirement |
|----|-------------|
| FR-CPN-001 | The system shall support coupons created by Organizers (their events) and by Super Admin (global). |
| FR-CPN-002 | Coupon fields: Code, Discount Type (Flat/Percentage), Discount Value, Max Cap, Min Booking Amount, Valid From, Valid To, Total Usage Limit, Per User Usage Limit, Applicable Events, Applicable Categories, Status. |
| FR-CPN-003 | Only one coupon per booking; no stacking. |
| FR-CPN-004 | Coupons shall not apply to free (₹0) bookings. |
| FR-CPN-005 | The system shall display user-friendly errors: invalid, expired, total usage exceeded, per-user limit reached, not applicable, min amount not met. |
| FR-CPN-006 | The system shall track coupon redemption stats (uses count, total discount given). |

## Module QRT — QR / E-Ticket

| ID | Requirement |
|----|-------------|
| FR-QRT-001 | The system shall generate one unique QR code per ticket. |
| FR-QRT-002 | QR payload shall be an HMAC-SHA256-signed opaque token mapping server-side to ticket id, booking id, event id, attendee name. |
| FR-QRT-003 | The system shall deliver QR via booking confirmation page download, email-attached PDF e-ticket, and dashboard re-download. |
| FR-QRT-004 | QR shall remain valid until the ticket is checked in, cancelled, or the event is completed/expired. |
| FR-QRT-005 | Re-download shall return the same QR token (no regeneration). |
| FR-QRT-006 | The system shall reject scan attempts after `event end_time + 2 hours`. |

## Module CHK — Check-in

| ID | Requirement |
|----|-------------|
| FR-CHK-001 | The system shall provide a browser-based mobile-friendly QR scanner accessible to assigned check-in staff. |
| FR-CHK-002 | Scan outcomes shall include: Valid, Already Checked In, Invalid, Wrong Event, Cancelled, Expired, Not Yet Active. |
| FR-CHK-003 | The system shall support manual lookup by booking reference, attendee name, or email. |
| FR-CHK-004 | The system shall enforce single-entry policy: subsequent scans of an already checked-in ticket shall be rejected. |
| FR-CHK-005 | The system shall require online connectivity (no offline mode in V1). |
| FR-CHK-006 | The system shall log every scan attempt: ticket id, scanned by, timestamp, outcome, optional device info. |

## Module NTF — Notifications

| ID | Requirement |
|----|-------------|
| FR-NTF-001 | The system shall send all notifications via SMTP using HTML templates. |
| FR-NTF-002 | Attendee emails: account verification, password reset, booking confirmation (with QR PDF), self-cancellation, organizer/admin event cancellation, event modification, 24-hr reminder, 2-hr reminder, post-event feedback request. |
| FR-NTF-003 | Organizer emails: account verification, approval/rejection/resubmission, suspension/reactivation, event publish confirmation, sold-out alert, event-completed summary. |
| FR-NTF-004 | Super Admin emails: new organizer pending approval, critical SMTP failure alerts. |
| FR-NTF-005 | Check-in staff emails: account creation, password reset, event assignment. |
| FR-NTF-006 | The system shall retry failed SMTP sends up to 3 times with exponential backoff and log failures. |

## Module RVW — Reviews

| ID | Requirement |
|----|-------------|
| FR-RVW-001 | Only attendees with at least one Checked-In ticket for the event shall be allowed to submit a review. |
| FR-RVW-002 | Reviews shall capture star rating (1–5) and a comment (≤ 500 chars). |
| FR-RVW-003 | Reviews shall auto-publish; Super Admin shall be able to remove abusive reviews. |
| FR-RVW-004 | Aggregate rating (average + count) shall display on the event public page and the organizer profile. |

## Module RPT — Reporting & Dashboards

| ID | Requirement |
|----|-------------|
| FR-RPT-001 | Super Admin dashboard shall display: total organizers (active/pending/suspended), total events by status, total bookings, total attendees, successful check-ins, coupon usage, failed email logs. |
| FR-RPT-002 | Organizer dashboard shall display: my events, bookings per event, sold vs available, expected revenue forecast, check-in stats, coupon performance, attendee CSV export, average ratings. |
| FR-RPT-003 | Attendee dashboard shall display: upcoming events, past events, cancelled bookings, attended events count. |
| FR-RPT-004 | The system shall provide CSV export for: attendee list, booking list, check-in audit, coupon usage, organizer event report. |
| FR-RPT-005 | Dashboards shall use KPI cards + tables; basic open-source charts permitted. |
| FR-RPT-006 | All dashboards shall support time-range filters (today / 7d / 30d / custom). |

## Module ADM — Admin Console

| ID | Requirement |
|----|-------------|
| FR-ADM-001 | Super Admin shall view, search, and filter all organizers. |
| FR-ADM-002 | Super Admin shall view, search, and filter all events. |
| FR-ADM-003 | Super Admin shall be able to Close any event (`Closed by Admin`) with mandatory reason. |
| FR-ADM-004 | Super Admin shall view audit logs with filters by entity, actor, date range. |
| FR-ADM-005 | Super Admin shall manage global coupons. |
| FR-ADM-006 | Super Admin shall view SMTP failure logs and re-trigger failed emails. |

## Module PLT — Platform

| ID | Requirement |
|----|-------------|
| FR-PLT-001 | The system shall expose static Privacy Policy and Terms & Conditions pages. |
| FR-PLT-002 | The system shall display a cookie / privacy notice banner on first visit. |
| FR-PLT-003 | The system shall expose a `/health` endpoint returning DB and SMTP connectivity status. |
| FR-PLT-004 | The system shall present an English UI with i18n-ready message keys. |
| FR-PLT-005 | The system shall format dates as `dd-MMM-yyyy HH:mm IST` and currency in Indian comma format with ₹ prefix. |

---

**End of FRS**
