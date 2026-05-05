# 08 — Business Rules & Edge Cases

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0
**Date:** 2026-05-05

> Each rule has an ID `BR-NNN`. Edge cases are documented as `EC-NNN`. These rules are normative — implementation must enforce them server-side.

---

## 1. Identity & Account Rules

| ID | Rule |
|----|------|
| BR-001 | Email is the unique login identifier across all roles. Two accounts cannot share an email. |
| BR-002 | Password must satisfy: length ≥ 8, ≥ 1 uppercase, ≥ 1 lowercase, ≥ 1 digit, ≥ 1 special character. |
| BR-003 | After 5 consecutive failed login attempts, the account is locked for 15 minutes (sliding window). |
| BR-004 | Verification link is single-use and expires after 24 hours. |
| BR-005 | Password reset link is single-use and expires after 24 hours. |
| BR-006 | JWT access token expires after 30 minutes of inactivity; refresh tokens expire after 7 days. |
| BR-007 | Deactivated attendee accounts retain transactional history for audit; profile fields are nulled / anonymized except for booking-attached attendee names. |
| BR-008 | Super Admin role is single; cannot be self-suspended. |

## 2. Organizer Rules

| ID | Rule |
|----|------|
| BR-101 | Organizer cannot create or publish events while in any non-Active status. |
| BR-102 | Sensitive-field edits (Org Name, PAN, GSTIN, Business Reg Cert) freeze publishing privileges until admin re-approves. |
| BR-103 | Suspension preserves all existing bookings; only an explicit event Cancel revokes them. |
| BR-104 | Suspension automatically disables all check-in staff accounts owned by that organizer. |
| BR-105 | Rejected organizers may reapply with the same email; previous rejection history is retained. |
| BR-106 | All organizer state transitions (prev, new, actor, reason, timestamp) must be persisted in audit log. |
| BR-107 | Verified Organizer badge is visible only on Active organizer profiles. |

## 3. Event Lifecycle Rules

| ID | Rule |
|----|------|
| BR-201 | Event start_time ≥ now + 2 hours at create or edit time. |
| BR-202 | Event end_time > start_time. |
| BR-203 | Event start_time ≤ now + 365 days. |
| BR-204 | Pre-publish requires: all mandatory fields, conditional fields, banner, ≥ 1 ticket category with qty > 0, valid future start. |
| BR-205 | Total capacity cannot be reduced below currently booked count. |
| BR-206 | Per-category quantity cannot be reduced below currently booked count for that category. |
| BR-207 | Date/time, venue, and online meeting link changes on a published event with bookings ≥ 1 must trigger an "Event Modified" attendee email. |
| BR-208 | Event category and event type changes after any booking exists are blocked at the API level (admin override only). |
| BR-209 | Online meeting link is visible only to attendees with at least one Confirmed ticket for the event. |
| BR-210 | Auto-completion job runs at most every 15 minutes; transitions Published events to Completed when end_time has passed. |
| BR-211 | QR scan validation is rejected after `event end_time + 2 hours` (grace window). |
| BR-212 | Unpublish blocks new bookings but preserves existing bookings; tokens remain valid. |
| BR-213 | Cancel cascades: all bookings → Cancelled by Organizer/Admin; all QR tokens invalidated; all attendees emailed. |

## 4. Ticket Inventory Rules

| ID | Rule |
|----|------|
| BR-301 | Maximum 10 ticket categories per event. |
| BR-302 | Ticket price ≥ 0 (free events allowed). |
| BR-303 | Min booking qty per category ≤ Max booking qty per category ≤ available qty. |
| BR-304 | Per-booking total tickets ≤ 10 (across all categories). |
| BR-305 | Inventory decrement must be atomic and use DB row locking; zero overselling is non-negotiable. |
| BR-306 | Sold Out flag is derived: `sum(available_qty across all categories) = 0`. |
| BR-307 | Inventory must be returned to availability on attendee/organizer/admin cancellation. |

## 5. Booking & Ticketing Rules

| ID | Rule |
|----|------|
| BR-401 | Booking requires a logged-in attendee; guest booking is not supported in V1. |
| BR-402 | Booking is atomic — no cart timeouts in V1. Inventory + booking + tickets + QR + audit + email-queue all commit in one transaction (email queue is best-effort delivery). |
| BR-403 | Booking reference format: `BK-YYYY-NNNNNN` where YYYY is current year and NNNNNN is a zero-padded sequential id (≥ 6 digits). |
| BR-404 | Each ticket in a multi-ticket booking carries its own attendee name and unique QR. |
| BR-405 | Attendee self-cancellation allowed only when `event.start_time - now > 24 hours`. |
| BR-406 | T&C acceptance is mandatory at booking confirmation. |
| BR-407 | Booking sale_window: defaults to publish-time → event start; organizer can override; can manually close early. |
| BR-408 | Booking after sale_end is blocked. |

## 6. Coupon Rules

| ID | Rule |
|----|------|
| BR-501 | Maximum 1 coupon per booking; no stacking. |
| BR-502 | Coupons cannot be applied to bookings with total amount = ₹0 (free booking). |
| BR-503 | Percentage coupons cap discount at `max_discount_cap` if set. |
| BR-504 | Coupon validation order: code exists → status Active → within validity dates → total uses < limit → per-user uses < limit → eligible event/category → min booking amount met. |
| BR-505 | Coupon code matching is case-insensitive; stored uppercase. |
| BR-506 | Each successful redemption increments total_uses and per-user counter atomically. |
| BR-507 | Organizer-scoped coupons apply only to that organizer's events; global coupons (Super Admin) apply to all events unless restricted. |

## 7. QR Ticketing Rules

| ID | Rule |
|----|------|
| BR-601 | One unique QR token per ticket (not per booking). |
| BR-602 | QR token is an HMAC-SHA256-signed opaque string; payload includes ticket UUID and issued-at timestamp. |
| BR-603 | The HMAC secret is stored only on the server; never embedded in client code or QR. |
| BR-604 | Re-download returns the same token; no rotation/regeneration. |
| BR-605 | Token verification is server-side only; scanner sends raw token to backend. |
| BR-606 | Tokens are invalidated immediately on cancellation (booking, event, or admin close). |

## 8. Check-in Rules

| ID | Rule |
|----|------|
| BR-701 | Check-in staff can validate only events to which they are explicitly assigned. |
| BR-702 | Single-entry policy: a `Checked In` ticket cannot be checked in again. |
| BR-703 | Check-in is allowed only within `event.start_time - 2h ≤ now ≤ event.end_time + 2h` (configurable per platform default in V1). |
| BR-704 | Manual lookup (by booking ref / name / email) is restricted to assigned check-in staff. |
| BR-705 | Every scan attempt — successful or not — must persist to scan_audit. |

## 9. Notification Rules

| ID | Rule |
|----|------|
| BR-801 | All emails delivered via SMTP using HTML branded templates. |
| BR-802 | Failed SMTP sends retry up to 3 times with exponential backoff (e.g., 1m, 5m, 30m). |
| BR-803 | After 3 failures, the email is logged as `Failed` and a critical-alert is queued for Super Admin. |
| BR-804 | Reminder emails: 24-hour reminder, 2-hour reminder, post-event feedback request. |
| BR-805 | Per-booking confirmation email is immediate; new-booking notifications to organizer are aggregated in the dashboard, not emailed per booking. |

## 10. Review & Feedback Rules

| ID | Rule |
|----|------|
| BR-901 | Reviews accepted only from attendees with at least one `Checked In` ticket for that event. |
| BR-902 | Star rating range: 1 to 5 inclusive. |
| BR-903 | Comment max length: 500 characters. |
| BR-904 | Reviews auto-publish; Super Admin may remove (soft-delete with reason). |
| BR-905 | Aggregate rating is `(sum_of_stars / count_of_reviews)` rounded to 1 decimal; count displayed alongside. |

## 11. Reporting & Export Rules

| ID | Rule |
|----|------|
| BR-1001 | Organizer dashboards show only data for that organizer's events; cross-tenant leakage is forbidden. |
| BR-1002 | Attendee dashboards show only that attendee's bookings. |
| BR-1003 | CSV exports honor the user's role-scoped query; no escalation. |
| BR-1004 | Time-range filters default to last 30 days. |

## 12. Audit Rules

| ID | Rule |
|----|------|
| BR-1101 | Audit log entries are immutable (insert-only). |
| BR-1102 | Audit captures: entity_type, entity_id, action, actor_id, actor_role, prev_value, new_value, reason, timestamp, request_id. |

---

## EDGE CASES

### EC-1xx — Identity / Auth
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-101 | User clicks expired verification link | Show "Link expired"; offer "Resend verification email" |
| EC-102 | User clicks already-used reset link | Show "Link already used"; offer fresh reset request |
| EC-103 | Concurrent sessions, password changed elsewhere | Active sessions remain valid until JWT expires; refresh fails afterward |
| EC-104 | User registers, never verifies, registers again with same email | Block; provide "resend verification" path |
| EC-105 | User attempts login during 15-min lockout | Reject with countdown; do not extend lock on each attempt |
| EC-106 | Refresh token used after 7 days | Reject; force re-login |

### EC-2xx — Organizer
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-201 | Organizer suspended mid-event with live attendees about to check in | Check-in staff (if disabled with organizer) cannot operate; admin must reactivate or assign manual workaround |
| EC-202 | Sensitive-field change submitted while events are live | Status moves to `Profile Update Pending Review`; existing events remain published; new publishing blocked |
| EC-203 | Document re-upload while in Resubmission Pending | Allowed; submission timestamp updated; queue re-sorted |
| EC-204 | Two simultaneous admin reviews on same organizer (race) | Optimistic concurrency: second action receives "stale state" error |

### EC-3xx — Event
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-301 | Organizer edits event start time to past | Reject (BR-201) |
| EC-302 | Organizer reduces capacity below booked count | Reject with explicit current-booked count |
| EC-303 | Organizer changes event type from Offline to Online after publish | Blocked (BR-208); requires admin override |
| EC-304 | Online event without a meeting link saved as Draft | Allowed; publish blocked until link provided |
| EC-305 | Cancellation email fails for some attendees | Per-recipient retry per BR-802; partial success acceptable |
| EC-306 | Auto-completion job fails for an event | Re-tried next cycle; failure logged; event remains Published until success |
| EC-307 | Event start_time exactly equals now + 2h boundary | Allowed (≥ 2h, inclusive) |

### EC-4xx — Ticket / Booking
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-401 | Two attendees confirm same last ticket simultaneously | Atomic decrement: one wins, other receives Sold Out (BR-305) |
| EC-402 | Attendee cancels at exactly 24h boundary | Allowed (≥ 24h, inclusive) |
| EC-403 | Per-category min qty is 2, attendee selects 1 | Validation error "Minimum 2 tickets for this category" |
| EC-404 | Booking total exceeds 10 tickets | Reject (BR-304) |
| EC-405 | Inventory restored after cancellation, then sale_end passed | Inventory available but no booking allowed (sale closed) |
| EC-406 | Organizer reduces a category's max_qty below an existing booking's count | Existing booking unaffected; new bookings limited by new max |
| EC-407 | Free booking attempt with coupon | Reject coupon (BR-502); allow booking without coupon |
| EC-408 | Attendee books, immediately cancels, attempts to rebook same category | Allowed; inventory was returned |
| EC-409 | Attendee email changed after booking | QR remains valid (mapped by ticket id); new emails go to new address |

### EC-5xx — Coupons
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-501 | Coupon expires while attendee is on review screen | Validate at confirmation time; reject if expired |
| EC-502 | Coupon total_usage_limit reached during concurrent bookings | Atomic increment; one succeeds, other gets "limit exceeded" |
| EC-503 | Percentage coupon discount > booking total | Discount capped at booking total (cannot make total negative) |
| EC-504 | Code entered with mixed case "Winter10" | Normalized to uppercase; matched (BR-505) |

### EC-6xx — QR / Check-in
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-601 | Attendee shares QR screenshot with friend | First scan checks in; subsequent scan rejected as "Already Checked In" (BR-702) |
| EC-602 | Camera fails on staff phone | Manual lookup fallback (BR-704) |
| EC-603 | Network drops during scan | Retry once; if still failing, instruct staff to retry; no offline mode in V1 |
| EC-604 | Same staff scans 2 different tickets within 1 second | Both processed independently; outcomes returned for each |
| EC-605 | Ticket cancelled while staff is mid-scan | Latest DB state wins; result "Cancelled" |
| EC-606 | Token signed with rotated/old secret | Reject as Invalid; secret rotation requires plan (Phase 2 keys table) |
| EC-607 | Scan attempted at exactly event_end + 2h boundary | Allowed (≤ inclusive) |

### EC-7xx — Notifications
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-701 | Attendee email bounces (hard fail) | Logged; no infinite retries; admin alert |
| EC-702 | SMTP throttling | Retry with backoff (BR-802) |
| EC-703 | Event modified twice within 5 min | Two notification emails sent (deduplication is Phase 2) |

### EC-8xx — Reviews
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-801 | Attendee with multiple tickets to same event submits multiple reviews | One review per (attendee, event); subsequent attempts update existing review |
| EC-802 | Admin removes review | Soft-delete; aggregate rating recomputed |

### EC-9xx — Misc / Concurrency
| ID | Edge Case | Expected Behavior |
|----|-----------|-------------------|
| EC-901 | DB connection lost mid-booking transaction | Transaction rolls back; no partial state; user receives retry-able error |
| EC-902 | File upload exceeds 2 MB | Reject before persisting; show clear error |
| EC-903 | File MIME mismatch (e.g., .exe renamed .jpg) | Reject (server-side MIME check) |
| EC-904 | Browser back button after booking confirmation | Idempotent — confirmation page shown; no duplicate booking |
| EC-905 | Two organizer-staff scans of the same QR within milliseconds | DB row lock ensures only one wins; other gets "Already Checked In" |

---

**End of Business Rules & Edge Cases**
