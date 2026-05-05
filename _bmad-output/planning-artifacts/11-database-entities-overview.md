# 11 — Database Entities Overview

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** V1.0 (Suggested logical model — final DDL by Architect)
**Date:** 2026-05-05
**Database:** MySQL 8.x

---

## 1. Conventions

- All PKs: `BIGINT UNSIGNED AUTO_INCREMENT`.
- All timestamps: `DATETIME(6)` stored UTC; display in IST.
- Money: `DECIMAL(12,2)` (INR).
- Strings: `VARCHAR` with explicit length; long text: `TEXT`.
- Soft-delete via `is_active` / `deleted_at` only where business demands.
- Audit-relevant tables include `created_at`, `updated_at`, `created_by`, `updated_by`.
- Booking reference & event public id may also expose a `UUID` for URLs.

---

## 2. Entity Catalog (Logical)

### 2.1 Identity

#### `users`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| email | VARCHAR(255) UNIQUE | login id |
| password_hash | VARCHAR(255) | BCrypt |
| role | ENUM('SuperAdmin','Organizer','Attendee','CheckinStaff') | |
| status | ENUM(...) | role-specific lifecycle states |
| email_verified_at | DATETIME(6) NULL | |
| failed_login_count | INT default 0 | |
| locked_until | DATETIME(6) NULL | |
| created_at, updated_at | DATETIME(6) | |

#### `attendee_profiles`
| Column | Type | Notes |
|--------|------|-------|
| user_id | BIGINT PK FK→users.id | |
| full_name | VARCHAR(150) | |
| phone | VARCHAR(20) | |
| city | VARCHAR(100) | |
| profile_image_path | VARCHAR(500) NULL | |

#### `organizer_profiles`
| Column | Type | Notes |
|--------|------|-------|
| user_id | BIGINT PK FK→users.id | |
| org_name | VARCHAR(200) | |
| contact_person | VARCHAR(150) | |
| phone | VARCHAR(20) | |
| address_line | VARCHAR(255) | |
| city, state, pincode | VARCHAR(...) | |
| event_category_hosted | VARCHAR(100) | |
| logo_path | VARCHAR(500) | |
| website_url | VARCHAR(500) NULL | |
| pan | VARCHAR(20) NULL | encrypted at rest (Phase 2 if needed) |
| gstin | VARCHAR(20) NULL | |
| pan_doc_path | VARCHAR(500) NULL | |
| gstin_doc_path | VARCHAR(500) NULL | |
| id_proof_path | VARCHAR(500) NULL | |
| business_reg_path | VARCHAR(500) NULL | |
| verified_badge | TINYINT(1) default 0 | |
| tier | VARCHAR(50) NULL | reserved for Phase 2 monetization |
| commission_rate | DECIMAL(5,2) NULL | reserved Phase 2 |

#### `checkin_staff_assignments`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| staff_user_id | BIGINT FK→users.id | |
| event_id | BIGINT FK→events.id | |
| organizer_user_id | BIGINT FK→users.id | scoping |
| created_at | DATETIME(6) | |

#### `password_reset_tokens` / `email_verification_tokens`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| user_id | BIGINT FK | |
| token_hash | VARCHAR(255) | hashed token |
| expires_at | DATETIME(6) | |
| consumed_at | DATETIME(6) NULL | single-use |

#### `refresh_tokens`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| user_id | BIGINT FK | |
| token_hash | VARCHAR(255) UNIQUE | |
| expires_at | DATETIME(6) | |
| revoked_at | DATETIME(6) NULL | |

---

### 2.2 Events & Tickets

#### `events`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| public_uuid | CHAR(36) UNIQUE | URL slug source |
| organizer_user_id | BIGINT FK→users.id | |
| title | VARCHAR(200) | |
| category | VARCHAR(50) | |
| description | TEXT | rich-text HTML, ≤3000 chars |
| event_type | ENUM('Offline','Online','Hybrid') | |
| start_time_utc | DATETIME(6) | |
| end_time_utc | DATETIME(6) | |
| timezone_display | VARCHAR(50) default 'Asia/Kolkata' | |
| visibility | ENUM('Public','Private') | |
| access_code_hash | VARCHAR(255) NULL | optional private code |
| total_capacity | INT | |
| status | ENUM('Draft','Published','Unpublished','Cancelled','Completed','ClosedByAdmin') | |
| sale_start_utc | DATETIME(6) NULL | |
| sale_end_utc | DATETIME(6) NULL | |
| banner_path | VARCHAR(500) | |
| tagline | VARCHAR(255) NULL | |
| event_language | VARCHAR(50) NULL | |
| age_guidance | VARCHAR(255) NULL | |
| terms_conditions | TEXT NULL | |
| website_url | VARCHAR(500) NULL | |
| social_links_json | JSON NULL | |
| online_meeting_link | VARCHAR(500) NULL | |
| recurrence_rule | VARCHAR(255) NULL | reserved Phase 2 |
| created_at, updated_at | DATETIME(6) | |

Indices: `(status, start_time_utc)`, `(organizer_user_id, status)`, `(category, city)` (city via venue join).

#### `event_venues`
| Column | Type | Notes |
|--------|------|-------|
| event_id | BIGINT PK FK→events.id | |
| venue_name | VARCHAR(200) | |
| address_line | VARCHAR(255) | |
| city | VARCHAR(100) | |
| state | VARCHAR(100) | |
| pincode | VARCHAR(10) | |

#### `event_gallery_images`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| event_id | BIGINT FK | |
| image_path | VARCHAR(500) | |
| sort_order | INT | |

#### `ticket_categories`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| event_id | BIGINT FK | |
| name | VARCHAR(100) | |
| price | DECIMAL(12,2) default 0 | INR |
| qty_total | INT | |
| qty_available | INT | atomically decremented |
| min_per_booking | INT default 1 | |
| max_per_booking | INT | |
| sale_start_utc | DATETIME(6) NULL | overrides event default |
| sale_end_utc | DATETIME(6) NULL | |
| description | VARCHAR(500) NULL | |

Constraint: `qty_available >= 0`. Index: `(event_id)`.

---

### 2.3 Bookings & Tickets

#### `bookings`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| reference | VARCHAR(20) UNIQUE | `BK-YYYY-NNNNNN` |
| public_uuid | CHAR(36) UNIQUE | |
| attendee_user_id | BIGINT FK→users.id | |
| event_id | BIGINT FK→events.id | |
| status | ENUM('Confirmed','CancelledByAttendee','CancelledByOrganizer','CancelledByAdmin','Completed','Expired') | |
| total_amount | DECIMAL(12,2) | gross |
| discount_amount | DECIMAL(12,2) default 0 | |
| net_amount | DECIMAL(12,2) | total - discount |
| coupon_id | BIGINT FK→coupons.id NULL | |
| payment_status | ENUM('NotApplicable','Pending','Paid','Refunded') default 'NotApplicable' | reserved Phase 2 |
| confirmed_at | DATETIME(6) | |
| cancelled_at | DATETIME(6) NULL | |
| cancellation_reason | VARCHAR(500) NULL | |
| terms_accepted | TINYINT(1) default 1 | |
| created_at | DATETIME(6) | |

Indices: `(attendee_user_id, status)`, `(event_id, status)`.

#### `booking_line_items`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| booking_id | BIGINT FK | |
| ticket_category_id | BIGINT FK | |
| quantity | INT | |
| unit_price | DECIMAL(12,2) | snapshot |
| line_total | DECIMAL(12,2) | |

#### `tickets`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| ticket_uuid | CHAR(36) UNIQUE | mapped from QR token |
| booking_id | BIGINT FK | |
| ticket_category_id | BIGINT FK | |
| event_id | BIGINT FK | denormalized for fast scan validation |
| attendee_name | VARCHAR(150) | per-ticket |
| qr_token_hash | VARCHAR(255) UNIQUE | hash of HMAC token for verification storage |
| status | ENUM('Issued','CheckedIn','Cancelled','Expired') | |
| checked_in_at | DATETIME(6) NULL | |
| checked_in_by | BIGINT FK→users.id NULL | |
| issued_at | DATETIME(6) | |

Indices: `(qr_token_hash)`, `(event_id, status)`.

---

### 2.4 Coupons

#### `coupons`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| code | VARCHAR(50) UNIQUE | uppercase normalized |
| owner_type | ENUM('Organizer','Global') | |
| owner_user_id | BIGINT FK→users.id NULL | null for Global |
| discount_type | ENUM('Flat','Percentage') | |
| discount_value | DECIMAL(12,2) | |
| max_discount_cap | DECIMAL(12,2) NULL | |
| min_booking_amount | DECIMAL(12,2) default 0 | |
| valid_from | DATETIME(6) | |
| valid_to | DATETIME(6) | |
| total_usage_limit | INT NULL | null = unlimited |
| total_uses | INT default 0 | atomic increment |
| per_user_usage_limit | INT default 1 | |
| status | ENUM('Active','Inactive','Expired') | |
| created_at | DATETIME(6) | |

#### `coupon_event_scopes`
| coupon_id | BIGINT FK |
| event_id | BIGINT FK |
PK (coupon_id, event_id). Empty rows = "applies to all" for global / all organizer events.

#### `coupon_category_scopes`
| coupon_id | BIGINT FK |
| ticket_category_id | BIGINT FK |
PK (coupon_id, ticket_category_id).

#### `coupon_redemptions`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| coupon_id | BIGINT FK | |
| user_id | BIGINT FK | |
| booking_id | BIGINT FK | |
| discount_applied | DECIMAL(12,2) | |
| redeemed_at | DATETIME(6) | |

---

### 2.5 Check-in & Audit

#### `scan_audit`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| ticket_id | BIGINT FK NULL | null if invalid token |
| event_id | BIGINT FK NULL | |
| scanned_by_user_id | BIGINT FK→users.id | |
| outcome | ENUM('Valid','AlreadyCheckedIn','Invalid','WrongEvent','Cancelled','Expired','NotYetActive','ManualCheckin') | |
| device_info | VARCHAR(255) NULL | UA string |
| scanned_at | DATETIME(6) | |

#### `audit_log`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| entity_type | VARCHAR(50) | e.g. 'Organizer','Event','Booking' |
| entity_id | BIGINT | |
| action | VARCHAR(100) | e.g. 'StatusChanged','SensitiveFieldEdited' |
| actor_user_id | BIGINT FK NULL | null = system |
| actor_role | VARCHAR(50) | |
| previous_value | JSON NULL | |
| new_value | JSON NULL | |
| reason | VARCHAR(500) NULL | |
| request_id | VARCHAR(100) NULL | correlation id |
| created_at | DATETIME(6) | |

Constraint: append-only (no UPDATE/DELETE permission to app user).

---

### 2.6 Reviews

#### `reviews`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| event_id | BIGINT FK | |
| attendee_user_id | BIGINT FK | |
| rating | TINYINT | 1–5 CHECK |
| comment | VARCHAR(500) NULL | |
| status | ENUM('Published','Removed') default 'Published' | |
| removed_by | BIGINT FK→users.id NULL | |
| removed_reason | VARCHAR(500) NULL | |
| created_at | DATETIME(6) | |

Unique `(event_id, attendee_user_id)` — enforces one review per attendee per event.

---

### 2.7 Notifications

#### `email_outbox`
| Column | Type | Notes |
|--------|------|-------|
| id | BIGINT PK | |
| recipient_email | VARCHAR(255) | |
| template_key | VARCHAR(100) | i18n-keyed template |
| payload_json | JSON | template variables |
| status | ENUM('Pending','Sent','Failed','Retrying') | |
| attempt_count | INT default 0 | |
| last_error | TEXT NULL | |
| scheduled_at | DATETIME(6) | |
| sent_at | DATETIME(6) NULL | |

#### `notification_preferences` *(reserved Phase 2 — opt-in/out)*
Stub table reserved.

---

### 2.8 Platform / Misc

#### `cookie_consent_log` *(optional)*
| user_id (nullable) | timestamp | accepted |

#### `system_settings`
| key | value | updated_by | updated_at |
Used for HMAC key id, reminder offsets, etc.

---

## 3. ER Overview (Logical)

```
users 1───* attendee_profiles
users 1───* organizer_profiles
users 1───* checkin_staff_assignments *───1 events

organizer_profiles 1───* events
events 1───1 event_venues
events 1───* event_gallery_images
events 1───* ticket_categories

users(attendee) 1───* bookings *───1 events
bookings 1───* booking_line_items *───1 ticket_categories
bookings 1───* tickets *───1 ticket_categories
bookings *───0..1 coupons

coupons 1───* coupon_event_scopes *───1 events
coupons 1───* coupon_category_scopes *───1 ticket_categories
coupons 1───* coupon_redemptions

tickets 1───* scan_audit
events 1───* reviews *───1 users(attendee)

audit_log (* polymorphic)  ───  any entity
email_outbox  ───  pending dispatch queue
```

---

## 4. Indexing & Performance Notes

| Table | Key Indices |
|-------|-------------|
| `events` | `(status, start_time_utc)`, `(organizer_user_id, status)`, `(visibility, status)`, FULLTEXT on `(title, description)` for keyword search |
| `tickets` | `(qr_token_hash)` UNIQUE, `(event_id, status)`, `(booking_id)` |
| `bookings` | `(attendee_user_id, status)`, `(event_id, status)`, `(reference)` UNIQUE |
| `ticket_categories` | `(event_id)` |
| `coupons` | `(code)` UNIQUE, `(status, valid_from, valid_to)` |
| `email_outbox` | `(status, scheduled_at)` |
| `audit_log` | `(entity_type, entity_id, created_at)` |
| `scan_audit` | `(event_id, scanned_at)` |

---

## 5. Forward-Compat Reserved Columns

| Table | Reserved Field | Phase 2 Use |
|-------|----------------|-------------|
| organizer_profiles | tier, commission_rate | Monetization plans |
| events | recurrence_rule | Recurring events |
| bookings | payment_status | Payment gateway |
| users | sso_provider, sso_subject | Google/Apple SSO |
| notifications | sms_optin, push_optin | SMS / push channels |

---

**End of Database Entities Overview**
