# 06 — Agile Epics & User Stories

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0
**Date:** 2026-05-05

> Stories follow the format: **As a** \<role\>, **I want** \<capability\>, **so that** \<outcome\>.
> Story IDs: `US-<EPIC>-NNN`. Acceptance criteria are detailed in Document 07.

---

## Epic Catalog

| Epic | Title | Modules |
|------|-------|---------|
| **E1** | Identity & Access Foundation | M01, M14, M17 |
| **E2** | Organizer Onboarding & Governance | M02, M14, M15 |
| **E3** | Event Authoring & Lifecycle | M03, M04, M15, M16 |
| **E4** | Public Discovery & Search | M05 |
| **E5** | Booking, Coupons & Ticketing | M06, M07, M08, M10 |
| **E6** | Check-in & Validation | M09, M14 |
| **E7** | Notifications & Reminders | M10, M16 |
| **E8** | Reviews & Feedback | M11 |
| **E9** | Reporting & Dashboards | M12, M13 |
| **E10** | Platform, Compliance & Ops | M17 |

---

## Epic E1 — Identity & Access Foundation

| Story | Title |
|-------|-------|
| US-E1-001 | As a **guest**, I want to register as an attendee with email, password, name, phone, and city so that I can book tickets. |
| US-E1-002 | As a **guest**, I want to register as an organizer with my organization details and optional documents so I can host events after approval. |
| US-E1-003 | As a **new user**, I want to verify my email via a link so that my account is activated. |
| US-E1-004 | As a **registered user**, I want to log in with my email and password so I can access my dashboard. |
| US-E1-005 | As a **user who forgot my password**, I want to reset it via email link so I can regain access. |
| US-E1-006 | As a **user**, I want to be temporarily locked out after 5 failed attempts so my account is protected from brute force. |
| US-E1-007 | As a **user**, I want my session to time out after 30 minutes of inactivity so my account stays secure on shared devices. |
| US-E1-008 | As a **user**, I want a "Remember Me" option so I don't need to re-login on my own device. |
| US-E1-009 | As an **attendee**, I want to view and edit my profile so my details remain current. |
| US-E1-010 | As an **organizer**, I want to view and edit my organization profile, with sensitive changes triggering admin re-review, so that data integrity is preserved. |
| US-E1-011 | As an **attendee**, I want to deactivate my account, while preserving my booking history, so I can leave the platform without losing audit trails. |
| US-E1-012 | As a **super admin**, I want to suspend or reactivate any user account so I can enforce policy. |

## Epic E2 — Organizer Onboarding & Governance

| Story | Title |
|-------|-------|
| US-E2-001 | As an **organizer**, I want to upload optional supporting documents (PAN, GSTIN, ID Proof, Reg Cert) so admins have stronger context. |
| US-E2-002 | As a **super admin**, I want a Pending Approval Queue sorted oldest first so I can clear the backlog efficiently. |
| US-E2-003 | As a **super admin**, I want to Approve an organizer and have them notified by email so they can start hosting. |
| US-E2-004 | As a **super admin**, I want to Reject an organizer with a mandatory reason so the decision is auditable. |
| US-E2-005 | As a **super admin**, I want to Request Resubmission with comments so onboarding isn't unnecessarily rigid. |
| US-E2-006 | As a **rejected organizer**, I want to reapply with the same email so I can correct and try again. |
| US-E2-007 | As a **super admin**, I want sensitive field edits (Org Name, PAN, GSTIN, Business Reg) by an active organizer to require my re-approval so identity drift is prevented. |
| US-E2-008 | As a **super admin**, I want to suspend an organizer with reason and reactivate them later so I can enforce discipline reversibly. |
| US-E2-009 | As a **public visitor**, I want to see a "Verified Organizer" badge on approved organizers so I can trust the listing. |
| US-E2-010 | As a **super admin**, I want every organizer state transition logged in audit so disputes can be resolved. |

## Epic E3 — Event Authoring & Lifecycle

| Story | Title |
|-------|-------|
| US-E3-001 | As an **organizer**, I want to create an event Draft with all mandatory and conditional fields so I can prepare it without going public. |
| US-E3-002 | As an **organizer**, I want to upload a banner (≤2 MB JPG/PNG) and up to 3 gallery images so my event looks professional. |
| US-E3-003 | As an **organizer**, I want to schedule events with start ≥ now+2h, end > start, and up to 1 year ahead so date integrity is enforced. |
| US-E3-004 | As an **organizer**, I want to create up to 10 ticket categories with price, qty, min/max, sale window so I can structure pricing. |
| US-E3-005 | As an **organizer**, I want to publish an event after pre-publish validation so I can avoid going live with errors. |
| US-E3-006 | As an **organizer**, I want to mark an event Public or Private (with optional access code) so I can target the right audience. |
| US-E3-007 | As an **organizer**, I want to edit a published event with restrictions (free / notify / restricted) so I can fix errors without breaking bookings. |
| US-E3-008 | As an **organizer**, I want to Unpublish an event temporarily without affecting existing bookings. |
| US-E3-009 | As an **organizer**, I want to Cancel an event, automatically invalidating all bookings & QR tickets, and notifying attendees. |
| US-E3-010 | As a **super admin**, I want to Close any event (with reason) so I can enforce platform policy. |
| US-E3-011 | As a **system**, I want to auto-mark events `Completed` after end-time so reporting is accurate. |
| US-E3-012 | As a **system**, I want QR scans rejected after end-time + 2h so late check-ins are blocked. |
| US-E3-013 | As an **organizer**, I want to capture an Online Meeting Link visible only to confirmed attendees so it isn't leaked publicly. |

## Epic E4 — Public Discovery & Search

| Story | Title |
|-------|-------|
| US-E4-001 | As a **guest**, I want to browse upcoming events on the homepage sorted soonest first so I can find what's relevant. |
| US-E4-002 | As a **guest**, I want to search events by keyword across title and description so I can find specific topics. |
| US-E4-003 | As a **guest**, I want to filter by category, city, date range, free/paid, online/offline/hybrid, language so I can narrow choices. |
| US-E4-004 | As a **guest**, I want to sort results by Soonest, Newest, A-Z so I can scan in my preferred order. |
| US-E4-005 | As a **guest**, I want to view category and city listing pages so I can browse contextually. |
| US-E4-006 | As an **invitee**, I want to access a private event via direct URL (and optional access code) so the organizer controls audience. |
| US-E4-007 | As a **guest**, I want a clear "Sold Out" badge on events with no inventory so I don't waste clicks. |

## Epic E5 — Booking, Coupons & Ticketing

| Story | Title |
|-------|-------|
| US-E5-001 | As an **attendee**, I want to book multiple ticket categories in one transaction so I can complete in fewer steps. |
| US-E5-002 | As an **attendee**, I want to provide a name for each ticket so each attendee gets their own QR. |
| US-E5-003 | As an **attendee**, I want to apply one coupon at booking with clear validation feedback so I get the discount or know why not. |
| US-E5-004 | As an **attendee**, I want to accept Terms & Conditions before confirming so I'm legally informed. |
| US-E5-005 | As an **attendee**, I want my booking confirmed atomically with no overselling, so my ticket is guaranteed. |
| US-E5-006 | As an **attendee**, I want my booking reference in `BK-YYYY-NNNNNN` format so it's easy to share with support. |
| US-E5-007 | As an **attendee**, I want a confirmation page with downloadable QR PDF and ICS calendar link so I'm ready for the event. |
| US-E5-008 | As an **attendee**, I want to view Upcoming, Past, and Cancelled bookings in a dashboard so I can track my activity. |
| US-E5-009 | As an **attendee**, I want to re-download my QR ticket from the dashboard so I can recover from lost emails. |
| US-E5-010 | As an **attendee**, I want to cancel my booking up to 24h before the event so I can change plans. |
| US-E5-011 | As an **organizer**, I want to create coupons for my events with all standard fields so I can run promotions. |
| US-E5-012 | As a **super admin**, I want to create global coupons applicable across events so I can run platform-wide campaigns. |
| US-E5-013 | As an **organizer / admin**, I want to see coupon redemption stats so I can measure promo effectiveness. |
| US-E5-014 | As a **system**, I want to issue HMAC-signed QR tokens that cannot be tampered, so tickets are secure. |
| US-E5-015 | As an **attendee**, I want my emailed PDF e-ticket to contain the same QR I see on the website so either works at entry. |

## Epic E6 — Check-in & Validation

| Story | Title |
|-------|-------|
| US-E6-001 | As an **organizer**, I want to create check-in staff accounts and assign them to one or more of my events so I can delegate entry. |
| US-E6-002 | As **check-in staff**, I want to log in to a mobile-friendly browser scanner so I don't need to install an app. |
| US-E6-003 | As **check-in staff**, I want to scan a QR and see one of: Valid / Already Checked In / Invalid / Wrong Event / Cancelled / Expired / Not Yet Active, in distinct colors, so I can act quickly. |
| US-E6-004 | As **check-in staff**, I want to manually look up by booking ref / attendee name / email so I can handle damaged QRs. |
| US-E6-005 | As **check-in staff**, I want re-scans of an already checked-in ticket to be rejected so single-entry is enforced. |
| US-E6-006 | As an **organizer / admin**, I want every scan attempt logged so I can audit disputes. |

## Epic E7 — Notifications & Reminders

| Story | Title |
|-------|-------|
| US-E7-001 | As an **attendee**, I want a verification email on signup so I can activate my account. |
| US-E7-002 | As an **attendee**, I want a booking confirmation email with QR PDF attached so I have my ticket offline. |
| US-E7-003 | As an **attendee**, I want a 24-hour and a 2-hour reminder email so I don't miss the event. |
| US-E7-004 | As an **attendee**, I want a notification email if the organizer changes date/time/venue/online link so I can adjust. |
| US-E7-005 | As an **attendee**, I want a feedback request email after the event so I can review my experience. |
| US-E7-006 | As an **organizer**, I want lifecycle emails (approval, rejection, suspension, sold-out, completion) so I'm kept informed. |
| US-E7-007 | As a **super admin**, I want a "new organizer pending" email and a "critical SMTP failure" alert so I can act in time. |
| US-E7-008 | As a **system**, I want failed SMTPs to retry 3 times with backoff and surface unrecoverable failures to admin so deliverability stays high. |

## Epic E8 — Reviews & Feedback

| Story | Title |
|-------|-------|
| US-E8-001 | As a **checked-in attendee**, I want to submit a 1-5 star rating + comment for the event so my experience is captured. |
| US-E8-002 | As a **public visitor**, I want to see aggregated rating + count on event pages and organizer profiles so I can judge quality. |
| US-E8-003 | As a **super admin**, I want to remove abusive reviews so platform quality is maintained. |

## Epic E9 — Reporting & Dashboards

| Story | Title |
|-------|-------|
| US-E9-001 | As a **super admin**, I want a dashboard with org/event/booking/check-in KPIs and time-range filters so I can monitor platform health. |
| US-E9-002 | As an **organizer**, I want a dashboard with my events, bookings, sold-vs-available, expected revenue, check-in stats, coupon performance, and average ratings. |
| US-E9-003 | As an **attendee**, I want a dashboard with upcoming/past/cancelled bookings and total events attended. |
| US-E9-004 | As an **organizer / admin**, I want CSV exports for attendee list, bookings, check-in audit, coupon usage so I can analyze offline. |
| US-E9-005 | As a **dashboard user**, I want KPI cards + tables + basic charts so I can read insights at a glance. |

## Epic E10 — Platform, Compliance & Ops

| Story | Title |
|-------|-------|
| US-E10-001 | As a **public visitor**, I want to read Privacy Policy and T&C pages so I'm legally informed. |
| US-E10-002 | As a **first-time visitor**, I want a cookie/privacy notice banner so I'm aware of data practices. |
| US-E10-003 | As an **operator**, I want a `/health` endpoint reporting DB and SMTP status so external monitors can ping it. |
| US-E10-004 | As a **developer**, I want all UI text behind i18n keys so future languages are easy to add. |
| US-E10-005 | As a **user**, I want all dates shown in `dd-MMM-yyyy HH:mm IST` and currency in Indian comma format with ₹ so the experience is locally familiar. |
| US-E10-006 | As an **operator**, I want structured JSON logs for auth, booking, check-in, admin actions, SMTP results so I can audit and debug. |
| US-E10-007 | As an **operator**, I want daily DB backups retained 30 days so I can recover from data loss. |

---

## Story Counts

| Epic | Stories | Estimated Sprint Count |
|------|---------|------------------------|
| E1 | 12 | 1 |
| E2 | 10 | 1 |
| E3 | 13 | 2 |
| E4 | 7 | 1 |
| E5 | 15 | 2 |
| E6 | 6 | 1 |
| E7 | 8 | 0.5 (cross-cutting) |
| E8 | 3 | 0.5 |
| E9 | 5 | 1 |
| E10 | 7 | 0.5 (cross-cutting) |
| **Total** | **86 stories** | **~9–10 sprints** |

---

**End of Epics & User Stories**
