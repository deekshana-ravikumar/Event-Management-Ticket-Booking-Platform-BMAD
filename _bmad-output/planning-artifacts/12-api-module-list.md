# 12 — API Module List Overview

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0 (suggested REST surface — final OpenAPI by Architect)
**Date:** 2026-05-05

---

## Conventions

- Base URL: `/api/v1`
- Auth: `Authorization: Bearer <jwt>` (except public endpoints)
- Content-Type: `application/json`
- Errors: `{ "code": "ERR_CODE", "message": "...", "details": [...] }` with appropriate HTTP status
- Pagination: `?page=1&pageSize=20`; max pageSize 100
- All datetimes in ISO-8601 UTC; UI converts to IST
- Idempotency: critical POSTs (booking confirm, cancel) accept `Idempotency-Key` header
- Rate limits documented per endpoint group

---

## 1. Auth & Identity (`/api/v1/auth`, `/api/v1/users`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/auth/register/attendee` | Public | Register attendee |
| POST | `/auth/register/organizer` | Public | Register organizer (with optional doc upload via separate endpoint) |
| GET | `/auth/verify-email?token=...` | Public | Verify email |
| POST | `/auth/resend-verification` | Public | Resend verification mail |
| POST | `/auth/login` | Public | Login → JWT + refresh |
| POST | `/auth/refresh` | Refresh token | Issue new access token |
| POST | `/auth/logout` | Bearer | Revoke refresh token |
| POST | `/auth/forgot-password` | Public | Request reset link |
| POST | `/auth/reset-password` | Public | Reset with token |
| GET | `/users/me` | Bearer | Current user profile |
| PUT | `/users/me/profile` | Bearer | Update profile (role-aware) |
| POST | `/users/me/profile/image` | Bearer | Upload profile image |
| POST | `/users/me/deactivate` | Bearer (Attendee) | Self-deactivation |
| POST | `/users/me/change-password` | Bearer | Change password |

---

## 2. Organizer Onboarding & Admin Approval (`/api/v1/organizers`, `/api/v1/admin/organizers`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/organizers/me/documents` | Bearer (Organizer) | Upload PAN / GSTIN / ID Proof / Reg Cert |
| GET | `/organizers/me` | Bearer (Organizer) | My organizer profile + status |
| PUT | `/organizers/me` | Bearer (Organizer) | Update profile (sensitive fields trigger re-review) |
| GET | `/admin/organizers/pending` | Bearer (Admin) | Pending approval queue |
| GET | `/admin/organizers` | Bearer (Admin) | List all organizers (filter: status, q) |
| GET | `/admin/organizers/{id}` | Bearer (Admin) | Detail view |
| POST | `/admin/organizers/{id}/approve` | Bearer (Admin) | Approve |
| POST | `/admin/organizers/{id}/reject` | Bearer (Admin) | Reject (reason required) |
| POST | `/admin/organizers/{id}/request-resubmission` | Bearer (Admin) | Request more info |
| POST | `/admin/organizers/{id}/suspend` | Bearer (Admin) | Suspend (reason required) |
| POST | `/admin/organizers/{id}/reactivate` | Bearer (Admin) | Reactivate |
| GET | `/organizers/{publicId}/profile` | Public | Public organizer profile + verified badge + ratings |

---

## 3. Events (`/api/v1/events`)

### Organizer-side
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/events` | Bearer (Organizer) | Create draft |
| GET | `/events/mine` | Bearer (Organizer) | List own events |
| GET | `/events/mine/{id}` | Bearer (Organizer) | Get own event |
| PUT | `/events/{id}` | Bearer (Organizer) | Update (validates edit-restrictions) |
| POST | `/events/{id}/banner` | Bearer (Organizer) | Upload banner |
| POST | `/events/{id}/gallery` | Bearer (Organizer) | Upload gallery image (max 3) |
| DELETE | `/events/{id}/gallery/{imageId}` | Bearer (Organizer) | Remove gallery image |
| POST | `/events/{id}/publish` | Bearer (Organizer) | Publish (runs pre-publish validation) |
| POST | `/events/{id}/unpublish` | Bearer (Organizer) | Unpublish |
| POST | `/events/{id}/cancel` | Bearer (Organizer) | Cancel (reason required) |
| POST | `/events/{id}/close-bookings` | Bearer (Organizer) | Manually close booking window early |

### Public discovery
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/events` | Public | List/search public events with filters & sort |
| GET | `/events/{publicId}` | Public | Event detail (online link hidden unless attendee booked) |
| GET | `/events/category/{category}` | Public | Category page |
| GET | `/events/city/{city}` | Public | City page |
| GET | `/events/private/{publicId}?accessCode=...` | Public | Private event direct access |

### Admin
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/admin/events` | Bearer (Admin) | List/search all events |
| POST | `/admin/events/{id}/close` | Bearer (Admin) | Close-by-admin (reason required) |

---

## 4. Ticket Categories (`/api/v1/events/{eventId}/categories`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/events/{eventId}/categories` | Bearer (Organizer) | Create category (≤10 per event) |
| GET | `/events/{eventId}/categories` | Public | List categories of an event |
| PUT | `/events/{eventId}/categories/{id}` | Bearer (Organizer) | Update (qty cannot go below booked) |
| DELETE | `/events/{eventId}/categories/{id}` | Bearer (Organizer) | Delete (only if 0 bookings) |

---

## 5. Bookings (`/api/v1/bookings`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/bookings/preview` | Bearer (Attendee) | Compute price + apply coupon (no commit) |
| POST | `/bookings/confirm` | Bearer (Attendee) | Atomic confirm; returns booking + tickets |
| GET | `/bookings/mine` | Bearer (Attendee) | Tabs: upcoming / past / cancelled |
| GET | `/bookings/mine/{reference}` | Bearer (Attendee) | Booking detail |
| POST | `/bookings/mine/{reference}/cancel` | Bearer (Attendee) | Self-cancel (≥24h before) |
| GET | `/bookings/mine/{reference}/tickets/{ticketId}/qr` | Bearer (Attendee) | Re-download QR (PNG) |
| GET | `/bookings/mine/{reference}/pdf` | Bearer (Attendee) | Download e-ticket PDF |
| GET | `/bookings/mine/{reference}/ics` | Bearer (Attendee) | Calendar ICS |

### Organizer-side
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/events/{eventId}/bookings` | Bearer (Organizer) | List event bookings |
| GET | `/events/{eventId}/bookings/export.csv` | Bearer (Organizer) | CSV export |

### Admin
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/admin/bookings` | Bearer (Admin) | List/search all bookings |

---

## 6. Coupons (`/api/v1/coupons`, `/api/v1/admin/coupons`)

### Organizer
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/coupons` | Bearer (Organizer) | Create organizer coupon |
| GET | `/coupons/mine` | Bearer (Organizer) | List own coupons |
| PUT | `/coupons/{id}` | Bearer (Organizer) | Update |
| DELETE | `/coupons/{id}` | Bearer (Organizer) | Deactivate |
| GET | `/coupons/{id}/redemptions` | Bearer (Organizer) | Redemption stats |

### Public application (within booking flow)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/coupons/validate` | Bearer (Attendee) | Validate code against cart payload |

### Admin
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/admin/coupons/global` | Bearer (Admin) | Create global coupon |
| GET | `/admin/coupons` | Bearer (Admin) | List all coupons |
| PUT | `/admin/coupons/{id}` | Bearer (Admin) | Update any |

---

## 7. Check-in (`/api/v1/checkin`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/checkin/my-events` | Bearer (CheckinStaff) | Events I am assigned to |
| POST | `/checkin/validate` | Bearer (CheckinStaff) | Validate scanned QR token; returns outcome |
| POST | `/checkin/manual` | Bearer (CheckinStaff) | Manual lookup + check-in |
| GET | `/checkin/lookup` | Bearer (CheckinStaff) | Search by booking ref / name / email |
| GET | `/events/{eventId}/checkin/audit` | Bearer (Organizer/Admin) | Scan audit log |
| GET | `/events/{eventId}/checkin/audit.csv` | Bearer (Organizer/Admin) | CSV export |

### Staff Management (Organizer)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/organizers/me/staff` | Bearer (Organizer) | Create check-in staff account (sends invite email) |
| GET | `/organizers/me/staff` | Bearer (Organizer) | List staff |
| PUT | `/organizers/me/staff/{id}` | Bearer (Organizer) | Update / disable |
| POST | `/organizers/me/staff/{id}/assign` | Bearer (Organizer) | Assign to event(s) |

---

## 8. Reviews (`/api/v1/reviews`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/events/{eventId}/reviews` | Bearer (Attendee, checked-in) | Submit / update review |
| GET | `/events/{eventId}/reviews` | Public | Paginated reviews + aggregate |
| GET | `/organizers/{publicId}/reviews` | Public | Aggregate organizer rating |
| DELETE | `/admin/reviews/{id}` | Bearer (Admin) | Remove abusive review (reason required) |

---

## 9. Reporting & Dashboards (`/api/v1/dashboard`, `/api/v1/admin/dashboard`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/dashboard/attendee` | Bearer (Attendee) | KPI: upcoming / past / cancelled / attended count |
| GET | `/dashboard/organizer` | Bearer (Organizer) | KPI summary + per-event metrics |
| GET | `/dashboard/organizer/events/{id}` | Bearer (Organizer) | Detailed event report |
| GET | `/admin/dashboard` | Bearer (Admin) | Platform KPIs |
| GET | `/admin/dashboard/audit-logs` | Bearer (Admin) | Filterable audit log viewer |
| GET | `/admin/dashboard/email-failures` | Bearer (Admin) | SMTP failure log |
| POST | `/admin/dashboard/email-failures/{id}/retry` | Bearer (Admin) | Manual retry |

### Time-range filter standard
All dashboard endpoints accept `?from=YYYY-MM-DD&to=YYYY-MM-DD` (default last 30 days).

---

## 10. CSV Exports (consolidated reference)

| Endpoint | Returns |
|----------|---------|
| `GET /events/{eventId}/bookings/export.csv` | Booking list |
| `GET /events/{eventId}/attendees/export.csv` | Attendee list per event |
| `GET /events/{eventId}/checkin/audit.csv` | Check-in audit |
| `GET /coupons/{id}/redemptions/export.csv` | Coupon usage |
| `GET /dashboard/organizer/events/{id}/report.csv` | Organizer event report |

---

## 11. Notifications (internal — not directly invoked by clients)

Internal services consume `email_outbox`; admin endpoints expose monitoring:
- `GET /admin/notifications/outbox` — view queue (filter status)
- `POST /admin/notifications/outbox/{id}/retry` — manual retry

---

## 12. Platform & Health (`/api/v1/platform`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health` | Public | DB + SMTP status |
| GET | `/platform/static/privacy-policy` | Public | Privacy policy content |
| GET | `/platform/static/terms` | Public | Terms & conditions |
| POST | `/platform/cookie-consent` | Public | Record cookie consent (optional) |

---

## 13. Standard Error Codes (samples)

| Code | HTTP | Meaning |
|------|------|---------|
| `AUTH_INVALID_CREDENTIALS` | 401 | Wrong email/password |
| `AUTH_ACCOUNT_LOCKED` | 423 | Locked due to failed attempts |
| `AUTH_EMAIL_NOT_VERIFIED` | 403 | Verify email first |
| `VALIDATION_FAILED` | 422 | Field errors (details list) |
| `INVENTORY_UNAVAILABLE` | 409 | Sold out / qty insufficient |
| `BOOKING_CANCEL_WINDOW_CLOSED` | 409 | Within 24h of event |
| `COUPON_INVALID` | 422 | Generic; details specify exact reason |
| `EVENT_NOT_PUBLISHABLE` | 422 | Pre-publish validation failed |
| `SCAN_INVALID_TOKEN` | 422 | Bad HMAC |
| `SCAN_WRONG_EVENT` | 409 | Token not for assigned event |
| `RATE_LIMITED` | 429 | Too many requests |
| `FORBIDDEN` | 403 | Authorization failure |
| `NOT_FOUND` | 404 | Entity missing |
| `CONFLICT_OPTIMISTIC` | 409 | Concurrent modification |

---

## 14. Rate Limit Suggested Defaults

| Endpoint group | Limit |
|-----------------|-------|
| `/auth/login` | 10 / min / IP |
| `/auth/forgot-password`, `/auth/resend-verification` | 5 / min / IP |
| `/coupons/validate` | 30 / min / user |
| `/checkin/validate` | 60 / min / staff |
| `/bookings/confirm` | 20 / min / user |
| Default | 120 / min / user |

---

**End of API Module List**
