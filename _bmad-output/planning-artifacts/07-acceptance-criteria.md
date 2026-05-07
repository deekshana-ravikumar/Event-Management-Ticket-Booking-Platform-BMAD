# 07 — Acceptance Criteria

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0
**Date:** 2026-05-05

> Acceptance criteria use **Gherkin** (Given / When / Then). Each story from Document 06 has at least one scenario covering the happy path plus key alternates. Edge cases are detailed in Document 08.

---

## E1 — Identity & Access Foundation

### US-E1-001 — Register Attendee
```gherkin
Scenario: Successful attendee registration
  Given I am on the attendee signup page
  When I submit valid Full Name, Email, Phone, City, and a strong password
  Then my account is created in "Pending Verification" status
  And a verification email is dispatched via SMTP
  And I am redirected to a "check your email" page

Scenario: Duplicate email rejected
  Given an active account exists with "user@example.com"
  When I attempt to register with the same email
  Then the system shows "Email already registered"
  And no new account is created

Scenario: Weak password rejected
  Given I am on the signup page
  When I submit a password "abc123"
  Then the system shows password policy errors
  And the form is not submitted
```

### US-E1-003 — Verify Email
```gherkin
Scenario: Successful email verification
  Given I am a "Pending Verification" attendee
  When I click the verification link within its 24-hr expiry
  Then my account becomes "Active"
  And I can now log in
```

### US-E1-004 — Login
```gherkin
Scenario: Successful login
  Given I have an Active account
  When I submit correct credentials
  Then a JWT access token (30 min) and refresh token are issued
  And I land on my role-based dashboard

Scenario: Unverified login blocked
  Given I am "Pending Verification"
  When I attempt to log in
  Then login is rejected with message "Please verify your email"
```

### US-E1-006 — Account Lockout
```gherkin
Scenario: Lockout after 5 failed attempts
  Given my account is Active
  When I submit an incorrect password 5 times consecutively
  Then my account is locked for 15 minutes
  And subsequent login attempts during lockout return "Account temporarily locked"
```

### US-E1-005 — Forgot Password
```gherkin
Scenario: Successful password reset
  Given I have an Active account
  When I request password reset for my email
  Then a single-use reset link valid for 24 hours is emailed
  When I open the link and submit a new compliant password
  Then my password is updated and I can log in
```

---

## E2 — Organizer Onboarding & Governance

### US-E2-002 / US-E2-003 — Approval Queue & Approve
```gherkin
Scenario: Admin approves an organizer
  Given an organizer is in "Pending Admin Approval"
  When the Super Admin clicks Approve in the queue
  Then the organizer status transitions to "Active"
  And an approval email is sent to the organizer
  And the action is recorded in audit log
```

### US-E2-004 — Reject with Reason
```gherkin
Scenario: Reject without reason fails
  Given an organizer is pending
  When the admin submits Reject with empty reason
  Then validation prevents submission

Scenario: Reject with reason succeeds
  When the admin submits Reject with reason "Incomplete documents"
  Then the organizer status becomes "Rejected"
  And rejection email containing the reason is dispatched
```

### US-E2-007 — Sensitive-Field Re-Review
```gherkin
Scenario: Sensitive field edit triggers re-review
  Given I am an Active organizer
  When I update my "Organization Name" or "PAN" or "GSTIN"
  Then my status changes to "Profile Update Pending Review"
  And I cannot publish new events until admin re-approves

Scenario: Free-edit field has no impact
  When I update my "Logo" or "Phone"
  Then my status remains "Active"
  And the change is saved immediately
```

### US-E2-008 — Suspension Preserves Bookings
```gherkin
Scenario: Suspended organizer's events stay live
  Given I am an organizer with 3 published events and existing bookings
  When the Super Admin suspends my account with a reason
  Then I cannot log in to my dashboard
  And my published events remain visible to attendees
  And existing bookings remain valid
  And my check-in staff accounts are disabled
  And a suspension email is sent to me
```

---

## E3 — Event Authoring & Lifecycle

### US-E3-005 — Pre-Publish Validation
```gherkin
Scenario: Publish blocked when validation fails
  Given my Draft event is missing a banner
  When I click Publish
  Then validation fails with "Banner required"
  And the event remains in Draft

Scenario: Successful publish
  Given my Draft event has all mandatory fields, a banner, and ≥1 ticket category with qty>0
  And event start is at least 2 hours in the future
  When I click Publish
  Then the event status becomes "Published"
  And it appears in public discovery
```

### US-E3-007 — Edit Restrictions on Published Events
```gherkin
Scenario: Reduce capacity below booked count is blocked
  Given my published event has total capacity 100 and 60 bookings
  When I try to set total capacity to 50
  Then the system rejects the change with "Capacity cannot be below already booked count (60)"

Scenario: Date change triggers attendee notification
  Given my published event has 25 bookings
  When I update the start date
  Then the event saves
  And an "Event Modified" email is queued for all 25 attendees
```

### US-E3-009 — Cancel Event
```gherkin
Scenario: Cancel cascades to bookings
  Given my published event has 50 confirmed bookings (100 tickets)
  When I cancel the event with reason "Venue unavailable"
  Then the event status becomes "Cancelled"
  And all 50 bookings transition to "Cancelled by Organizer"
  And all 100 ticket QR tokens become invalid
  And cancellation emails are queued for 50 attendees

Scenario: Scan attempt on cancelled-event ticket (CR-1)
  Given an event was cancelled by its organizer
  And a ticket for that event was previously "Issued" with a valid QR
  When check-in staff scans the QR
  Then the system returns scan outcome "Cancelled"
  And UI shows red "Cancelled—Event was cancelled by organizer"
  And no state change occurs
  And the scan attempt is recorded in the scan audit log
```

### US-E3-011 — Auto-Completion
```gherkin
Scenario: Event auto-completed after end-time
  Given an event ended at 18:00 IST today
  When the scheduler runs after 18:00 IST
  Then the event status becomes "Completed"
  And post-event feedback emails are queued for attendees who checked in
```

---

## E4 — Public Discovery & Search

### US-E4-002 — Keyword Search
```gherkin
Scenario: Keyword matches title or description
  Given a public event titled "AI Conference 2026"
  And a public event with description containing "AI"
  When a guest searches "AI"
  Then both events appear in results
```

### US-E4-006 — Private Event Access
```gherkin
Scenario: Private event hidden from search
  Given an event is set to Private
  When a guest searches by its title
  Then the event does not appear in search results

Scenario: Private event accessible by direct URL
  When a guest visits the event's direct URL
  Then the event detail page loads
  And booking is allowed (subject to optional access code)
```

---

## E5 — Booking, Coupons & Ticketing

### US-E5-001 — Multi-Category Booking
```gherkin
Scenario: Book General + VIP in one transaction
  Given an event has General (₹500, qty 10) and VIP (₹2000, qty 5) categories
  When I add 2 General + 1 VIP and confirm
  Then a single booking is created with 3 tickets across 2 categories
  And inventory becomes General=8, VIP=4
```

### US-E5-002 — Per-Ticket Attendee Names
```gherkin
Scenario: Names required for each ticket
  Given I am booking 3 tickets
  When I attempt to confirm with names for only 2 tickets
  Then the system blocks confirmation with "Name required for each attendee"

Scenario: Each ticket gets its own QR
  When I confirm 3 tickets with 3 attendee names
  Then 3 unique QR tokens are generated, one per ticket
```

### US-E5-005 — Zero Overselling
```gherkin
Scenario: Concurrent bookings cannot exceed inventory
  Given a category has qty 1 remaining
  When two attendees confirm 1 ticket each at the same time
  Then exactly one booking succeeds
  And the other receives "Sold Out / inventory unavailable"
```

### US-E5-010 — Cancellation Window
```gherkin
Scenario: Cancellation allowed ≥24h before event
  Given my booking is for an event 30 hours from now
  When I cancel
  Then booking and all tickets become "Cancelled"
  And QR tokens are invalidated
  And inventory is returned
  And I receive a cancellation email

Scenario: Cancellation blocked within 24h
  Given my booking is for an event 12 hours from now
  When I attempt to cancel
  Then the system rejects with "Cancellation window closed"

Scenario: Scan attempt on attendee-cancelled ticket (CR-2)
  Given I cancelled my booking earlier today (event is tomorrow)
  And I still possess my original QR PDF
  When check-in staff scans the QR
  Then the system returns scan outcome "Cancelled"
  And UI shows red "Cancelled—Booking cancelled by attendee"
  And no state change occurs
  And the scan attempt is recorded in the scan audit log
```

### US-E5-003 — Coupon Validation Errors
```gherkin
Scenario: Expired coupon
  Given coupon "WINTER10" expired yesterday
  When I apply it
  Then the system shows "Coupon expired"
  And no discount is applied

Scenario: Per-user limit reached
  Given coupon "FIRST10" allows 1 use per user and I already used it
  When I apply it
  Then "You have already used this coupon" is shown

Scenario: Coupon blocked on free booking
  Given my booking total is ₹0
  When I apply any coupon
  Then "Coupon cannot be applied to free bookings" is shown
```

### US-E5-014 — HMAC-Signed QR
```gherkin
Scenario: Tampered QR rejected at scan
  Given a valid QR token
  When the token payload is altered by 1 character and rescanned
  Then HMAC verification fails
  And the scan result is "Invalid Ticket"
```

---

## E6 — Check-in & Validation

### US-E6-002 / US-E6-003 — Scan Outcomes
```gherkin
Scenario: Valid scan
  Given a ticket is "Issued" for event E
  And I (staff) am assigned to event E
  And current time is between event start-2h and end+2h
  When I scan its QR
  Then the ticket transitions to "Checked In"
  And UI shows green ✅ with attendee name and category

Scenario: Already checked in
  Given the ticket was already checked in 5 minutes ago
  When I scan it again
  Then UI shows orange "Already Checked In at HH:MM"
  And no state change occurs

Scenario: Wrong event
  Given I am assigned to event A
  When I scan a ticket issued for event B
  Then UI shows red "Wrong Event"

Scenario: Not yet active
  Given current time is more than 2h before event start
  When I scan a valid ticket
  Then UI shows orange "Not Yet Active — valid from HH:MM"

Scenario: Cancelled ticket
  Given a ticket was cancelled
  When I scan it
  Then UI shows red "Cancelled Ticket"
  And the scan outcome "Cancelled" applies whether the cancellation source was attendee-self-cancel (US-E5-010) OR organizer-event-cancel (US-E3-009)

Scenario: Expired
  Given event ended 3 hours ago (past 2h grace)
  When I scan an Issued ticket
  Then UI shows red "Expired"
```

### US-E6-004 — Manual Lookup
```gherkin
Scenario: Lookup by booking reference
  Given staff cannot scan due to damaged QR
  When staff searches "BK-2026-000123"
  Then the booking and tickets are listed
  And staff can mark Checked In manually
  And the audit log records "manual" outcome
```

---

## E7 — Notifications

### US-E7-002 — Booking Confirmation Email
```gherkin
Scenario: Confirmation email with PDF
  Given I successfully book 2 tickets
  When the booking is confirmed
  Then within 60 seconds I receive an email with subject containing the event title
  And a PDF e-ticket is attached
  And the PDF contains 2 QR codes (one per ticket)
```

### US-E7-008 — SMTP Retry
```gherkin
Scenario: Retry failed sends
  Given the SMTP service is temporarily unavailable
  When the system attempts to send an email
  Then it retries up to 3 times with exponential backoff
  And after 3 failures, the email is logged as failed
  And the Super Admin receives a critical-failure alert
```

---

## E8 — Reviews

### US-E8-001 — Review Eligibility
```gherkin
Scenario: Only checked-in attendees can review (offline / hybrid events)
  Given event E is offline or hybrid
  And I have a ticket marked "Checked In" for event E
  When I open the event page
  Then I see a Review form

Scenario: Online event—no scan required (CR-3)
  Given event E is an online event
  And event E has status "Completed"
  And I have a confirmed (non-cancelled) booking for event E
  When I open the event page
  Then I see a Review form
  # Online events skip QR check-in entirely; eligibility = Confirmed booking + event Completed

Scenario: Online event not yet completed
  Given event E is an online event with status "Published" (not yet Completed)
  And I have a confirmed booking for event E
  When I open the event page
  Then no Review form is shown

Scenario: Non-attendee blocked
  Given I never checked in for event E (offline) OR I have no confirmed booking (online)
  When I open the event page
  Then no Review form is shown
  And direct API attempt returns 403

Scenario: Cancelled booking blocked from reviewing online event
  Given event E is online and Completed
  And my booking for event E was Cancelled
  When I open the event page
  Then no Review form is shown

Scenario: Comment length enforced
  When I submit a comment of 600 characters
  Then validation rejects with "Maximum 500 characters"
```

**Cross-cutting note for online events (CR-3):** Online events bypass the entire check-in flow (US-E6-001..006). The scanner UI is not relevant for online events; check-in staff are not required; QR codes are still issued (for consistency and proof-of-purchase) but are never scanned. Review eligibility for online events is computed as `booking.Status = Confirmed AND event.Status = Completed`.

---

## E9 — Reporting

### US-E9-002 — Organizer Dashboard
```gherkin
Scenario: Sold vs Available
  Given my event has Category A (qty 100, sold 30) and Category B (qty 50, sold 50)
  When I open the organizer dashboard for that event
  Then sold = 80, available = 70
  And expected revenue = (30 × A.price) + (50 × B.price)
```

### US-E9-004 — CSV Export
```gherkin
Scenario: Attendee list CSV
  Given my event has 100 confirmed tickets
  When I click "Export Attendees CSV"
  Then a CSV download begins
  And it contains 100 rows with columns: Booking Ref, Ticket ID, Attendee Name, Email, Phone, Category, Status, Checked-In At
```

---

## E10 — Platform & Ops

### US-E10-003 — Health Endpoint
```gherkin
Scenario: Health endpoint reports OK
  Given DB and SMTP are reachable
  When GET /health is called
  Then response is 200 with JSON {"status":"OK","db":"OK","smtp":"OK","time":"..."}

Scenario: Health endpoint reports DB failure
  Given DB is unreachable
  When GET /health is called
  Then response is 503 with JSON status "DEGRADED" and db "FAIL"
```

### US-E10-005 — Locale Formatting
```gherkin
Scenario: Indian currency format
  When the system displays the amount 100000
  Then it renders as "₹1,00,000.00"

Scenario: Date in IST
  When the system displays an event start of 2026-06-15T19:30:00+05:30
  Then it renders as "15-Jun-2026 19:30 IST"
```

---

## NFR Acceptance Examples

```gherkin
Scenario: Booking confirmation latency
  Given moderate concurrent load (50 concurrent users)
  When a booking is confirmed
  Then end-to-end response time is < 2 seconds at P95

Scenario: QR scan latency
  Given a check-in scan
  Then validation response is < 1 second at P95

Scenario: HTTPS enforced
  When I navigate to http://<domain>/
  Then I am redirected to https://<domain>/
  And HSTS header is present
```

---

**End of Acceptance Criteria**
