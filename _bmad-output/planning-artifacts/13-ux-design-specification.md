# 13 — UX Design Specification

**Project:** Smart Event Management & Ticket Booking Platform
**Author:** Sally (UX Designer)
**Date:** 2026-05-06
**Target:** Angular Web Application — Responsive, Mobile-first
**Scope:** V1.0 MVP — All Roles

---

## Table of Contents

1. [UX Principles & Design Foundation](#1-ux-principles--design-foundation)
2. [Role Definitions & Mental Models](#2-role-definitions--mental-models)
3. [Screen & Page Inventory](#3-screen--page-inventory)
4. [Navigation Hierarchy](#4-navigation-hierarchy)
5. [User Journey Maps](#5-user-journey-maps)
6. [Wireframe-Level Screen Descriptions](#6-wireframe-level-screen-descriptions)
7. [Booking Flow UX](#7-booking-flow-ux)
8. [QR Scanner & Check-in UX](#8-qr-scanner--check-in-ux)
9. [Dashboard UX Layouts](#9-dashboard-ux-layouts)
10. [Form Field UX Recommendations](#10-form-field-ux-recommendations)
11. [Responsive & Mobile Behavior](#11-responsive--mobile-behavior)
12. [UX Consistency Guidelines for Angular Developers](#12-ux-consistency-guidelines-for-angular-developers)

---

## 1. UX Principles & Design Foundation

### 1.1 Core UX Principles

| Principle | Applied Meaning |
|-----------|----------------|
| **Progressive disclosure** | Show only what the user needs at this moment. Event authoring: step-by-step. Booking: one-screen summary. Check-in: full-screen, single outcome. |
| **Instant feedback** | Every action produces a visual response within 100 ms (optimistic UI where safe, spinner otherwise). |
| **Error prevention over error correction** | Validate inline, before submit. Disable submit until required fields pass. Show character counters. |
| **Reversibility** | Make destructive actions (Cancel Booking, Cancel Event, Suspend Organizer) two-tap with explicit confirmation and consequence statement. |
| **Consistent affordance** | Same action = same component everywhere. "Primary CTA" = filled button in brand colour. "Danger action" = outlined red button. |
| **Mobile-first, desktop-enhanced** | Design the mobile layout first. Desktop is a wider, richer version of the mobile layout — not the other way around. |

### 1.2 Design System Tokens (Angular Material base)

```
Primary colour:     #1E40AF  (deep blue — trust, authority)
Secondary colour:   #0EA5E9  (sky blue — energy, events)
Success/valid:      #16A34A  (green)
Warning:            #D97706  (amber)
Danger/error:       #DC2626  (red)
Neutral background: #F8FAFC
Card surface:       #FFFFFF
Text primary:       #0F172A
Text secondary:     #64748B
Border:             #E2E8F0
Focus ring:         2px solid #1E40AF
Border radius base: 8px
Spacing unit:       8px grid
```

### 1.3 Typography Scale

```
H1: 28px / 700 — page titles
H2: 22px / 600 — section headers
H3: 18px / 600 — card headers
Body: 16px / 400 — body text
Small: 14px / 400 — captions, helper text
Micro: 12px / 400 — metadata, timestamps
```

### 1.4 Breakpoints

```
Mobile (xs):   0 – 599px       → 1-column layout, bottom nav
Tablet (sm):   600 – 959px     → 2-column grid, side nav collapsible
Desktop (md+): 960px+          → sidebar fixed, content up to 1200px max-width
```

---

## 2. Role Definitions & Mental Models

### 2.1 Roles

| Role | Mental Model | Primary Goal in One Sentence |
|------|-------------|------------------------------|
| **Guest (unauthenticated)** | Window shopper | "Let me find something interesting to attend." |
| **Attendee** | Ticket holder | "Book fast, have my ticket ready, not miss the event." |
| **Organizer** | Event producer | "Publish my event, fill seats, see who showed up." |
| **Check-in Staff** | Gatekeeper | "Scan every QR quickly and confidently. No tech drama at the door." |
| **Super Admin** | Platform operator | "Keep the platform trustworthy, handle escalations, see platform health." |

### 2.2 Role Access Map

| Area | Guest | Attendee | Organizer | Check-in Staff | Super Admin |
|------|-------|----------|-----------|---------------|-------------|
| Public event browsing | ✅ | ✅ | ✅ | — | ✅ |
| Event detail page | ✅ | ✅ | ✅ | — | ✅ |
| Booking | ❌ (redirect to login) | ✅ | ❌ | — | — |
| Organizer portal | — | — | ✅ | — | — |
| Check-in scanner | — | — | — | ✅ | — |
| Super Admin console | — | — | — | — | ✅ |
| Attendee dashboard | — | ✅ | — | — | — |
| Organizer dashboard | — | — | ✅ | — | — |
| Admin dashboard | — | — | — | — | ✅ |

---

## 3. Screen & Page Inventory

### 3.1 Public / Unauthenticated Screens (Guest + all roles)

| ID | Screen | Route | Description |
|----|--------|-------|-------------|
| PUB-01 | Homepage | `/` | Hero, upcoming events grid, search bar, category strips |
| PUB-02 | Search Results | `/events/search` | Filtered list with sidebar filters |
| PUB-03 | Category Page | `/events/category/:slug` | Events by category |
| PUB-04 | City Page | `/events/city/:city` | Events by city |
| PUB-05 | Event Detail | `/events/:slug` | Full event info, ticket categories, booking CTA |
| PUB-06 | Register — Attendee | `/register/attendee` | Sign up form |
| PUB-07 | Register — Organizer | `/register/organizer` | Multi-step org onboarding form |
| PUB-08 | Login | `/login` | Email + password, Remember Me |
| PUB-09 | Forgot Password | `/forgot-password` | Email entry |
| PUB-10 | Reset Password | `/reset-password?token=` | New password entry |
| PUB-11 | Email Verification | `/verify-email?token=` | Auto-redirect, status message |
| PUB-12 | Privacy Policy | `/privacy` | Static page |
| PUB-13 | Terms & Conditions | `/terms` | Static page |
| PUB-14 | 404 / Not Found | `**` | Friendly error page |

### 3.2 Attendee Portal Screens

| ID | Screen | Route | Description |
|----|--------|-------|-------------|
| ATT-01 | Attendee Dashboard | `/my/dashboard` | KPIs, upcoming bookings, quick actions |
| ATT-02 | My Bookings — Upcoming | `/my/bookings/upcoming` | Tab: confirmed future bookings |
| ATT-03 | My Bookings — Past | `/my/bookings/past` | Tab: completed bookings |
| ATT-04 | My Bookings — Cancelled | `/my/bookings/cancelled` | Tab: cancelled bookings |
| ATT-05 | Booking Detail | `/my/bookings/:ref` | Full detail, QR download, cancel CTA |
| ATT-06 | Booking Flow — Select Tickets | `/events/:slug/book` | Step 1: quantity + names |
| ATT-07 | Booking Flow — Coupon & Review | `/events/:slug/book/review` | Step 2: coupon, T&C, total |
| ATT-08 | Booking Confirmation | `/booking/confirmed/:ref` | Step 3: success page, QR, email sent |
| ATT-09 | Profile Settings | `/my/profile` | Edit personal info |
| ATT-10 | Change Password | `/my/profile/password` | Current + new password |
| ATT-11 | Cancel Booking | Modal overlay on ATT-05 | Two-step confirmation modal |

### 3.3 Organizer Portal Screens

| ID | Screen | Route | Description |
|----|--------|-------|-------------|
| ORG-01 | Organizer Dashboard | `/organizer/dashboard` | KPIs: events, bookings, revenue display, check-ins, ratings |
| ORG-02 | My Events List | `/organizer/events` | Table/cards of organizer's events with status badges |
| ORG-03 | Create Event — Step 1: Basic Info | `/organizer/events/new/basic` | Title, category, type, dates |
| ORG-04 | Create Event — Step 2: Details | `/organizer/events/new/details` | Description, venue/link, media |
| ORG-05 | Create Event — Step 3: Tickets | `/organizer/events/new/tickets` | Ticket categories CRUD |
| ORG-06 | Create Event — Step 4: Review & Publish | `/organizer/events/new/review` | Pre-publish validation summary |
| ORG-07 | Event Detail / Edit | `/organizer/events/:id` | Edit fields respecting publish-state restrictions |
| ORG-08 | Event Attendees | `/organizer/events/:id/attendees` | Attendee list + CSV export |
| ORG-09 | Event Check-in Stats | `/organizer/events/:id/checkin` | Real-time scan summary |
| ORG-10 | Coupons List | `/organizer/coupons` | All coupons for organizer's events |
| ORG-11 | Create / Edit Coupon | `/organizer/coupons/new` or `/organizer/coupons/:id/edit` | Coupon form |
| ORG-12 | Check-in Staff | `/organizer/staff` | List + create check-in staff accounts |
| ORG-13 | Organizer Profile | `/organizer/profile` | Edit non-sensitive fields; sensitive-field submit triggers re-review notice |
| ORG-14 | Organizer Notifications | `/organizer/notifications` | Inbox: approval updates, booking alerts, SMTP fails |
| ORG-15 | Pending Re-review Banner | Global banner injected on all ORG pages when status = Profile Update Pending Review | Inline warning |

### 3.4 Check-in Staff Screens

| ID | Screen | Route | Description |
|----|--------|-------|-------------|
| CHK-01 | Staff Login | `/checkin/login` | Dedicated login — minimal UI |
| CHK-02 | Event Selection | `/checkin/events` | List of assigned events (usually 1) |
| CHK-03 | Scanner — Active | `/checkin/events/:id/scan` | Full-screen camera + outcome panel |
| CHK-04 | Manual Lookup | `/checkin/events/:id/lookup` | Booking ref / name / email search |
| CHK-05 | Scanner — Outcome Detail | Overlay on CHK-03 | Attendee detail card after scan |

### 3.5 Super Admin Console Screens

| ID | Screen | Route | Description |
|----|--------|-------|-------------|
| ADM-01 | Admin Dashboard | `/admin/dashboard` | Platform KPIs |
| ADM-02 | Organizer Queue | `/admin/organizers/pending` | Pending approval list |
| ADM-03 | Organizer Detail / Review | `/admin/organizers/:id` | View docs, Approve/Reject/Resubmit |
| ADM-04 | All Organizers | `/admin/organizers` | Full list with status filters |
| ADM-05 | Organizer Re-review Queue | `/admin/organizers/rereviews` | Profile-update pending review |
| ADM-06 | All Users | `/admin/users` | Attendees + organizers search |
| ADM-07 | User Detail | `/admin/users/:id` | Suspend/Reactivate, history |
| ADM-08 | All Events | `/admin/events` | Search + filter all platform events |
| ADM-09 | Event Detail (Admin View) | `/admin/events/:id` | Close event, view bookings |
| ADM-10 | All Bookings | `/admin/bookings` | Search by ref, attendee, event |
| ADM-11 | Coupon Management | `/admin/coupons` | Global coupons CRUD |
| ADM-12 | Reviews Moderation | `/admin/reviews` | Flag queue + remove action |
| ADM-13 | Audit Log | `/admin/audit` | Filterable audit event log |
| ADM-14 | SMTP / System Health | `/admin/system` | Health status, SMTP queue, failed emails |
| ADM-15 | Admin Profile / Password | `/admin/profile` | — |

**Total screens: 14 (public) + 11 (attendee) + 15 (organizer) + 5 (check-in) + 15 (admin) = 60 screens**

---

## 4. Navigation Hierarchy

### 4.1 Public Navigation (Header)

```
[Logo]                     [Events ▾]  [Login]  [Register ▾]
                            ├─ Browse All
                            ├─ By Category
                            └─ By City
                                            └─ As Attendee
                                               As Organizer
```

**Mobile:** Hamburger → slide-in drawer with same items.

### 4.2 Attendee Navigation (Post-login Header)

```
[Logo]    [Browse Events]    [My Bookings]    [Avatar ▾ — Profile | Logout]
```

**Mobile:** Bottom tab bar:
```
[🏠 Explore]  [🎟 My Tickets]  [👤 Profile]
```

### 4.3 Organizer Portal (Side Navigation)

```
[Platform Logo]
─────────────────────────
📊 Dashboard
📅 My Events
   ├─ All Events
   └─ [+ Create Event]
🎫 Coupons
👥 Check-in Staff
🔔 Notifications
─────────────────────────
⚙ Profile
🚪 Logout
```

**Mobile:** Collapsible sidebar → hamburger icon at top. Active route highlighted.

### 4.4 Check-in Staff Navigation

Minimal. No side nav.
```
[← Back to Events]    [Event Name]    [Manual Lookup]
```
Scanner fills the rest of the viewport. No distractions.

### 4.5 Super Admin Console (Side Navigation)

```
[Admin Console]
─────────────────────────
📊 Dashboard
👥 Organizers
   ├─ Pending Queue  [badge: count]
   ├─ Re-review Queue [badge: count]
   └─ All Organizers
👤 Users
📅 Events
🎟 Bookings
🏷 Coupons
⭐ Reviews
📋 Audit Log
🖥 System Health
─────────────────────────
⚙ Profile
🚪 Logout
```

---

## 5. User Journey Maps

### 5.1 Journey: Attendee Books a Ticket

```
STAGE         Discover          Evaluate          Book              Receive           Attend
─────────────────────────────────────────────────────────────────────────────────────────────
ACTION        Searches          Opens event       Selects           Gets email        Shows QR
              Google/Home       detail page       tickets +         with PDF +        at gate;
              page              Reads info        applies coupon    QR attached       scanned ✅

TOUCHPOINT    Homepage /        PUB-05            ATT-06 → 07 → 08  Email client      CHK-03
              Search results    Event Detail      Booking flow       PDF e-ticket      Scanner

EMOTION       Curious 😊        Interested 🤔     Slight friction    Relief 😌         Excited 🎉
                                                  on login if
                                                  not logged in

PAIN POINT    Too many          Not enough        Login wall         PDF not          WiFi might
              irrelevant        info about        if not already     on phone         fail
              results           organizer         logged in

DESIGN FIX    Smart filters     Verified badge,   Redirect to        QR shows on      Manual
              on discovery      rating visible    login → return     confirmation     lookup
                                prominently       URL preserved      page too         fallback
```

### 5.2 Journey: Organizer Publishes First Event

```
STAGE     Register     Wait for      Create         Publish         Monitor
                       Approval      Event
────────────────────────────────────────────────────────────────────────────
ACTION    Fills org    Receives       Fills 4-step    Clicks         Watches
          form +       approval       event wizard    Publish        bookings
          uploads docs email ✅                       → Live ✅       come in

EMOTION   Hopeful      Anxious        Building 🏗     Proud 🎉        Nervous/
                       (waiting)      excitement      relieved        excited

PAIN POINT Lots of     No ETA on      Uncertainty     Fear of         Data
           fields      approval       about           errors          hard to
           to fill                    required fields                 parse

DESIGN FIX Progress   Status page    Step progress   Pre-publish     Clean
           indicator  shows state    bar + live       validation      dashboard
           on form    + ETA text     validation       list before     with KPI
                                     per step         submit          cards
```

### 5.3 Journey: Check-in Staff at Event Gate

```
STAGE     Arrive      Login          Scan           Handle          Close
          at venue                   tickets        exception       event
─────────────────────────────────────────────────────────────────────────
ACTION    Opens       Staff login    Holds phone    Attendee has    Last scan;
          browser     page →         camera up →    damaged QR;     all done
          on phone    event list     scan → green   manual lookup

EMOTION   Rushed ⚡   Quick and      Confident      Stressed 😰     Relieved
                      efficient

PAIN POINT WiFi       Password       Camera slow    Slow search      —
           patchy     forgotten      to focus       by name

DESIGN FIX Offline    Simple form;   Large camera   Auto-search     Scanner
           graceful   auto-remember  viewport;      as you type     shows
           fallback   on that device fast feedback  (instant)       event-end
           shown                                                     time
```

### 5.4 Journey: Super Admin Reviews Organizer Application

```
STAGE     Receive      Open           Review         Decide         Notify
          alert        application    docs
──────────────────────────────────────────────────────────────────────────
ACTION    Email: "New  Admin queue    Sees org info  Approve /      Auto email
          org pending" → click        + docs         Reject /       sent by
                                      previewed      Resubmit       system

EMOTION   Neutral      Focused        Evaluating     Decisive       Done ✅

PAIN POINT Many         Not easy to    Docs don't     Hard to        No
           pending      navigate to    preview        write          follow-up
           at once      right one      inline         rejection      reminder

DESIGN FIX Badge on     Sorted         Inline doc     Mandatory      Audit
           nav item     oldest first   viewer in      reason         log entry
                                       same page      + template     auto-made
```

---

## 6. Wireframe-Level Screen Descriptions

### 6.1 PUB-01 — Homepage

```
┌─────────────────────────────────────────────────────────────────┐
│  HEADER: Logo | Browse Events | Login | Register                 │
├─────────────────────────────────────────────────────────────────┤
│  HERO SECTION (full-width, ~300px tall)                          │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  "Discover Events Near You"                               │  │
│  │  [Search bar — 60% width]         [🔍 Find Events]       │  │
│  │  Popular: Music  Tech  Workshop  Sports  Comedy           │  │
│  └───────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│  UPCOMING EVENTS  [See all →]                                    │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │
│  │ CARD     │ │ CARD     │ │ CARD     │ │ CARD     │           │
│  │ Banner   │ │ Banner   │ │ Banner   │ │ Banner   │           │
│  │ Title    │ │ ...      │ │ ...      │ │ ...      │           │
│  │ Date IST │ │          │ │          │ │          │           │
│  │ City     │ │          │ │          │ │          │           │
│  │ ₹ / Free │ │          │ │          │ │          │           │
│  │ [Book]   │ │          │ │          │ │          │           │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘           │
├─────────────────────────────────────────────────────────────────┤
│  BROWSE BY CATEGORY                                              │
│  [Music] [Tech] [Workshop] [Sports] [Arts] [Food] [Comedy] ...  │
├─────────────────────────────────────────────────────────────────┤
│  FOOTER: Privacy | T&C | Cookie Notice | © 2026                 │
└─────────────────────────────────────────────────────────────────┘
```

**Key UX rules:**
- Event cards are 4-up on desktop, 2-up on tablet, 1-up on mobile.
- "Sold Out" badge overlays the banner image (grey wash + "SOLD OUT" text).
- "Free" badge in green pill; paid events show "₹X onwards".
- Verified Organizer badge (blue checkmark) below organizer name on card.
- Date always in `dd-MMM-yyyy HH:mm IST` format.

---

### 6.2 PUB-05 — Event Detail Page

```
┌─────────────────────────────────────────────────────────────────┐
│ HEADER                                                           │
├──────────────────────────────────────┬──────────────────────────┤
│  EVENT BANNER (full width, 16:9)     │  BOOKING CARD (sticky)   │
│                                      │  ┌────────────────────┐  │
│  [← Back]     [Share]                │  │ Ticket Categories  │  │
│                                      │  │ General: ₹500  [10]│  │
│  Event Title (H1)                    │  │ VIP: ₹2000     [5] │  │
│  🗓 Sat, 10-Jun-2026 06:00PM IST     │  │                    │  │
│  📍 Venue Name, Chennai              │  │ [Book Now]         │  │
│  🏷 Category   👤 Verified Org ✓     │  │ or [Login to Book] │  │
│  ⭐ 4.3 (42 reviews)                 │  │                    │  │
│                                      │  │ Booking closes:    │  │
│  TABS: [About] [Tickets] [Reviews]   │  │ 10-Jun-2026 05:00  │  │
│                                      │  └────────────────────┘  │
│  Tab: About                          │                          │
│  ─ Description (rich text)           │                          │
│  ─ Organizer card (name, logo, badge)│                          │
│                                      │                          │
│  Tab: Tickets                        │                          │
│  ─ Category table                    │                          │
│                                      │                          │
│  Tab: Reviews (post-event)           │                          │
│  ─ Rating bar + reviews list         │                          │
└──────────────────────────────────────┴──────────────────────────┘
```

**Key UX rules:**
- Booking card is sticky on desktop (right column). On mobile: sticky at bottom as a bar → taps to expand.
- If event is `Sold Out`, booking card shows "SOLD OUT — Join Waitlist (V2)" placeholder greyed out with "All tickets sold" message.
- If event is `Cancelled`, booking card replaced with red banner: "This event has been cancelled."
- If event is `Completed`, booking card replaced with "This event has ended."
- Online meeting link section shows: "Meeting link visible to confirmed attendees after booking" — not shown publicly.
- Private events accessed via direct URL show normal detail page (no discovery entry).

---

### 6.3 PUB-06 — Attendee Registration

```
┌────────────────────────────────────────────┐
│  Create your account                        │
│                                             │
│  Full Name *          [_________________]   │
│  Email *              [_________________]   │
│  Phone *              [+91 _____________]   │
│  City *               [_________________]   │
│  Password *           [_________________]   │
│    ↳ strength meter: [●●●○○] Medium         │
│  Confirm Password *   [_________________]   │
│                                             │
│  ☐ I accept the [Terms & Conditions]        │
│                                             │
│  [Create Account]                           │
│                                             │
│  Already have an account? [Login]           │
└────────────────────────────────────────────┘
```

**After submit:** "Check your email! We sent a verification link to [email]. It expires in 24 hours."

---

### 6.4 PUB-07 — Organizer Registration (Multi-step)

**Step indicator:** `[1] Basic Info → [2] Business Details → [3] Documents → [4] Review`

**Step 1 — Basic Info:**
```
Org Name *         Logo * (upload)
Contact Person *   Event Category *
Email *            Phone *
Password *         Confirm Password *
```

**Step 2 — Business Details:**
```
Business Address *    City *    State *    Pincode *
Website URL           Social Links (Instagram, Facebook, Twitter)
Short Bio (≤300 chars)
```

**Step 3 — Documents (all optional):**
```
PAN Card           [Upload PDF/JPG]
GSTIN              [Upload PDF/JPG]
ID Proof           [Upload PDF/JPG]
Business Reg Cert  [Upload PDF/JPG]

ℹ Documents help us verify your identity faster.
  Max 2MB per file. PDF, JPG, PNG accepted.
```

**Step 4 — Review:**
```
Summary of entered data
[← Edit] [Submit Application]
```

**After submit:** Full-page status card: "Application Submitted! We'll review within 48 hours. Check [email] for updates."

---

### 6.5 ORG-03 to ORG-06 — Event Creation Wizard

**Step indicator:** `[1] Basic Info → [2] Event Details → [3] Tickets → [4] Review & Publish]`

**Step 1 — Basic Info:**
```
Event Title *          [__________________________________]  (80 char limit, counter shown)
Category *             [Dropdown: Music / Tech / ...]
Event Type *           [○ Offline  ○ Online  ○ Hybrid]
Start Date & Time *    [Date picker]  [Time picker]  IST
End Date & Time *      [Date picker]  [Time picker]  IST
Visibility *           [○ Public  ○ Private (unlisted)]
```

**Step 2 — Event Details:**
```
Description *          [Rich text editor, 3000 char limit + counter]
Tagline                [__________________________] (optional)
Language               [Dropdown]
Age Guidance           [All / 18+ / etc]

─── Venue (shown only if Offline or Hybrid) ───
Venue Name *           [______________]
Address *              [______________]
City *                 [______________]
State *                [______________]
Pincode *              [______________]

─── Online (shown only if Online or Hybrid) ───
Meeting Link *         [______________]
(ℹ Visible only to confirmed attendees)

─── Media ───
Banner *               [Upload JPG/PNG ≤2MB]   [Preview thumbnail]
Gallery                [Upload up to 3 images] [Thumbnails]

─── Optional ───
Website URL            [______________]
Instagram / Facebook   [______________]
T&C for this event     [Text area]
Booking Opens          [default: on publish]
Booking Closes         [default: event start]
```

**Step 3 — Ticket Categories:**
```
[+ Add Ticket Category]

For each category card:
┌──────────────────────────────────────────────────────────────┐
│  Category Name *     [________________]                       │
│  Price (₹) *         [0 = Free]   (Display price — informational) │
│  Qty Available *     [___]                                    │
│  Min per booking     [1]   Max per booking  [10]              │
│  Sale Start          [Date/Time] (optional)                   │
│  Sale End            [Date/Time] (optional)                   │
│  Description         [________________________]               │
│                                           [🗑 Remove]         │
└──────────────────────────────────────────────────────────────┘

Max 10 categories. Counter: "3 / 10 categories"
```

**Step 4 — Review & Publish:**
```
Pre-publish checklist:
✅ Title set
✅ Banner uploaded
✅ Start time valid (≥ now + 2h)
✅ At least 1 ticket category with qty > 0
⚠  Description is short (< 100 chars) — consider expanding
              [← Back to Edit]   [🚀 Publish Now]   [Save as Draft]
```

---

### 6.6 ADM-02 — Organizer Approval Queue

```
┌─────────────────────────────────────────────────────────────────────┐
│  Pending Organizer Approvals  [23 pending]                          │
│  Sorted: Oldest first  [Filter: All / Docs Provided / No Docs]      │
├──────────┬──────────────┬──────────┬──────────┬────────┬───────────┤
│ Org Name │ Contact      │ Category │ Submitted│ Docs   │ Action    │
├──────────┼──────────────┼──────────┼──────────┼────────┼───────────┤
│ ABC Evts │ Raj Kumar    │ Music    │ 3 days   │ ✅ 3/4 │ [Review]  │
│ XYZ Prod │ Priya S.     │ Tech     │ 1 day    │ ❌ 0/4 │ [Review]  │
│ ...      │ ...          │ ...      │ ...      │ ...    │ ...       │
└──────────┴──────────────┴──────────┴──────────┴────────┴───────────┘
```

**ADM-03 — Organizer Detail / Review:**
```
Left panel: Org info (all fields)
Right panel: Document viewer (PDF/image inline)

Action bar (sticky bottom):
[✅ Approve]  [❌ Reject]  [🔄 Request Resubmission]

Rejection / Resubmission → opens modal:
┌──────────────────────────────────────────┐
│  Rejection Reason *                       │
│  [Text area — min 20 chars]               │
│  [Cancel]  [Confirm Reject]               │
└──────────────────────────────────────────┘
```

---

## 7. Booking Flow UX

### 7.1 Flow Overview

```
Event Detail Page (PUB-05)
    │
    ▼ [Book Now] — must be logged in (if not: redirect to /login?returnUrl=...)
    │
ATT-06 — Select Tickets
    │
    ▼ [Continue]
    │
ATT-07 — Review & Confirm
    │
    ▼ [Confirm Booking] — spinner → atomic backend confirm
    │
ATT-08 — Booking Confirmed ✅
    │
    ▼ (async) Confirmation email + PDF sent
```

### 7.2 ATT-06 — Select Tickets (Step 1)

```
┌────────────────────────────────────────────────────────────────────┐
│  Booking: [Event Name]                              Step 1 of 2    │
│  🗓 Sat 10-Jun-2026  📍 Chennai                                     │
├────────────────────────────────────────────────────────────────────┤
│  SELECT TICKETS                                                     │
│                                                                     │
│  General Admission    ₹500              (8 left)                    │
│  [−] [  2  ] [+]     ₹1,000                                        │
│  ┌─ Attendee 1 ──────────────────────────────────────────────────┐ │
│  │  Name *  [___________________________]                         │ │
│  └───────────────────────────────────────────────────────────────┘ │
│  ┌─ Attendee 2 ──────────────────────────────────────────────────┐ │
│  │  Name *  [___________________________]                         │ │
│  └───────────────────────────────────────────────────────────────┘ │
│                                                                     │
│  VIP                  ₹2,000            (2 left)                    │
│  [−] [  1  ] [+]     ₹2,000                                        │
│  ┌─ Attendee 1 ──────────────────────────────────────────────────┐ │
│  │  Name *  [___________________________]                         │ │
│  └───────────────────────────────────────────────────────────────┘ │
│                                                                     │
│  Tickets: 3/10 (max 10 per booking)  ←── BR-NEW-01 progress        │
│                                                                     │
│  Subtotal: ₹3,000                   [Continue →]                   │
└────────────────────────────────────────────────────────────────────┘
```

**Key UX rules:**
- Attendee name fields expand dynamically when quantity > 0.
- [−] button disabled at min quantity. [+] disabled at max OR when remaining inventory = 0.
- "X left" shown when inventory ≤ 5 (urgency signal).
- Category with 0 inventory: greyed out row + "SOLD OUT" chip.
- Ticket cap counter: "Tickets: 3/10" — turns amber at 8+, red at 10.
- Price shown as "₹0 — Free" for zero-price categories.
- Do NOT show any payment-related language anywhere. No "Pay now", no "Payment method".

---

### 7.3 ATT-07 — Review & Confirm (Step 2)

```
┌────────────────────────────────────────────────────────────────────┐
│  Booking: [Event Name]                              Step 2 of 2    │
├────────────────────────────────────────────────────────────────────┤
│  BOOKING SUMMARY                                                    │
│  ───────────────                                                    │
│  General × 2   ₹1,000                                              │
│  VIP × 1       ₹2,000                                              │
│  ─────────────────────                                             │
│  Subtotal      ₹3,000                                              │
│                                                                     │
│  COUPON                                                             │
│  [Enter coupon code ___________]  [Apply]                          │
│  ✅ EARLY20 applied — ₹600 off                                      │
│  ─────────────────────                                             │
│  Total         ₹2,400  (display only — no payment collected)       │
│                                                                     │
│  ☐ I accept the [Terms & Conditions] *                             │
│                                                                     │
│  [← Back]                    [Confirm Booking]                      │
│                               (spinner on tap)                      │
└────────────────────────────────────────────────────────────────────┘
```

**Coupon UX rules:**
- Inline validation on Apply: shows success (green), or specific error message in red.
- Error messages exactly: "Invalid coupon", "Coupon expired", "Usage limit reached", "Already used this coupon", "Minimum booking amount not met", "Not applicable for this event/category", "Cannot apply to free booking".
- Rate-limited silently — if too many attempts, show "Too many attempts, please wait a moment."
- "Total" line includes a subtle note: "Display total — this platform does not collect payments in V1."
- [Confirm Booking] button: disabled until T&C checked + all attendee names filled.
- On click: button → loading state "Confirming..." — never double-submittable (disabled after first click).

---

### 7.4 ATT-08 — Booking Confirmation (Step 3)

```
┌────────────────────────────────────────────────────────────────────┐
│  ✅  Booking Confirmed!                                             │
│                                                                     │
│  Reference:  BK-2026-004821                                        │
│  Event:      [Event Name]                                           │
│  Date:       Sat, 10-Jun-2026 06:00 PM IST                         │
│  Venue:      [Venue / Online]                                       │
│                                                                     │
│  ─── Your Tickets ──────────────────────────────────────────────── │
│  🎟 General — Raj Kumar          [Download QR PDF]                  │
│  🎟 General — Priya S.           [Download QR PDF]                  │
│  🎟 VIP — Thisanth K.            [Download QR PDF]                  │
│                                                                     │
│  📧 Confirmation email with all QR codes sent to you@email.com     │
│                                                                     │
│  [View All My Bookings]    [Back to Events]                         │
└────────────────────────────────────────────────────────────────────┘
```

**Key UX rules:**
- Confirmation page is the single source of truth for the user. Never just show a toast.
- QR PDF download available inline — do not force them to go to dashboard.
- For online events: meeting link shown here (only to confirmed attendees): "📹 Online Meeting Link: [link] — Don't share this link."
- Back/refresh on this page must be safe — idempotency handled by backend.

---

## 8. QR Scanner & Check-in UX

### 8.1 CHK-01 — Staff Login

```
┌──────────────────────────────┐
│                              │
│  [Platform Logo]             │
│                              │
│  Check-in Staff Login        │
│                              │
│  Email   [_______________]   │
│  Password[_______________]   │
│                              │
│  [Login]                     │
│                              │
│  ℹ This portal is for        │
│  check-in staff only.        │
└──────────────────────────────┘
```

Minimal. No registration link. No "Forgot password" (staff accounts managed by organizer).

---

### 8.2 CHK-02 — Event Selection

```
┌────────────────────────────────┐
│  Your Assigned Events          │
│                                │
│  ┌────────────────────────┐   │
│  │ 🎵 Rock Night 2026     │   │
│  │ Today, 06:00 PM IST    │   │
│  │ Venue: Phoenix Mall    │   │
│  │ [Start Check-in →]     │   │
│  └────────────────────────┘   │
└────────────────────────────────┘
```

If only 1 event assigned → auto-redirect to scanner.

---

### 8.3 CHK-03 — Active Scanner (the most critical UX in the system)

```
┌────────────────────────────────────────────────────────┐
│  [← Events]    Rock Night 2026       [Manual Lookup]   │
├────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │                                                  │  │
│  │          CAMERA VIEWFINDER                       │  │
│  │          (full width, 70% viewport height)       │  │
│  │                                                  │  │
│  │          ┌──────────┐                            │  │
│  │          │  QR aim  │                            │  │
│  │          │  frame   │                            │  │
│  │          └──────────┘                            │  │
│  │                                                  │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
│  OUTCOME PANEL (slides up from bottom after each scan) │
│  ┌──────────────────────────────────────────────────┐  │
│  │  ✅  VALID                                        │  │
│  │  Raj Kumar — General Admission                   │  │
│  │  BK-2026-004821 · Ticket #1                      │  │
│  │  [Auto-dismiss in 3s]  [✓ Done]                  │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
│  Scanned today: 142 / 200                              │
└────────────────────────────────────────────────────────┘
```

### 8.4 Scan Outcome Color System (7 outcomes)

| Outcome | Background | Icon | Title | Message |
|---------|-----------|------|-------|---------|
| ✅ Valid | `#16A34A` green | ✅ | **VALID** | `{name} — {category}` |
| 🟠 Already Checked In | `#D97706` amber | ⚠ | **ALREADY CHECKED IN** | "Checked in at HH:MM IST" |
| 🔴 Invalid | `#DC2626` red | ✗ | **INVALID TICKET** | "QR code not recognised" |
| 🔴 Wrong Event | `#DC2626` red | ✗ | **WRONG EVENT** | "This ticket is for [Event Name]" |
| 🔴 Cancelled | `#DC2626` red | ✗ | **CANCELLED** | "Booking cancelled by [attendee/organizer]" |
| 🟠 Not Yet Active | `#D97706` amber | ⏰ | **NOT YET ACTIVE** | "Valid from HH:MM IST" |
| 🔴 Expired | `#6B7280` grey | ✗ | **EXPIRED** | "Event ended" |

**Key UX rules:**
- Outcome panel covers bottom 30% of screen — visible without scrolling.
- Auto-dismiss after 3 seconds; staff can also tap "Done" to dismiss immediately.
- Sound feedback (optional if browser supports): high-pitched short beep = valid, low buzz = invalid.
- Scanner re-activates automatically after dismiss. No tap needed to resume scanning.
- Counter at bottom shows scanned / total capacity.
- Full-screen mode recommended — suggest entering fullscreen on first open.

---

### 8.5 CHK-04 — Manual Lookup

```
┌─────────────────────────────────────────────────────────┐
│  [← Scanner]    Manual Lookup                           │
│                                                         │
│  [Search by booking ref, attendee name, or email...]    │
│  (auto-searches after 2 chars)                          │
│                                                         │
│  Results:                                               │
│  ┌───────────────────────────────────────────────────┐  │
│  │ Raj Kumar · BK-2026-004821 · General              │  │
│  │ Status: Issued (not yet checked in)               │  │
│  │ [✓ Mark as Checked In]                            │  │
│  ├───────────────────────────────────────────────────┤  │
│  │ Priya S. · BK-2026-004821 · General               │  │
│  │ Status: ✅ Checked In at 06:14 PM IST              │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

**Key UX rules:**
- Search is instant (debounced 300ms).
- Already checked-in tickets are greyed out with timestamp; cannot be marked again.
- Manual check-in logs audit entry with `source=manual`.

---

## 9. Dashboard UX Layouts

### 9.1 Attendee Dashboard (ATT-01)

```
┌─────────────────────────────────────────────────────────────────────┐
│  Hi Thisanth! 👋                                                     │
├──────────────┬──────────────┬──────────────┬────────────────────────┤
│ 📅 Upcoming  │ 🎟 Total     │ ✅ Attended  │ ❌ Cancelled            │
│     3        │ Bookings 12  │     9        │      1                 │
└──────────────┴──────────────┴──────────────┴────────────────────────┘
│                                                                      │
│  UPCOMING EVENTS                                                     │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │ 🎵 Rock Night 2026              Sat 10-Jun-2026 06:00 PM    │    │
│  │ General × 2   BK-2026-004821                                │    │
│  │ [View Tickets]  [Download QR]  [Cancel Booking]             │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                                                                      │
│  [View All Bookings →]                                               │
└──────────────────────────────────────────────────────────────────────┘
```

**Tabs below KPIs:** [Upcoming] [Past] [Cancelled]

---

### 9.2 Organizer Dashboard (ORG-01)

```
┌──────────────────────────────────────────────────────────────────────┐
│  Dashboard — TechTalks India                         [+ New Event]   │
├──────────┬──────────┬──────────┬──────────┬──────────┬──────────────┤
│ 📅 Total │ 🟢 Live  │ 🎟 Total │ ✅ Checked│ 💰 Revenue│ ⭐ Avg Rating │
│ Events   │ Events   │ Bookings │ In        │ Display  │              │
│   12     │    3     │   284    │   142     │ ₹1,40,000│  4.3 / 5     │
└──────────┴──────────┴──────────┴──────────┴──────────┴──────────────┘
│                                                                       │
│  MY LIVE EVENTS                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │ Event Name    Date          Booked/Cap   Check-ins  Status     │ │
│  │ Rock Night    10-Jun-2026   84/100 ▓▓▓▓░ 42/84     Published  │ │
│  │ AI Summit     15-Jun-2026   120/200      0          Published  │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                       │
│  RECENT BOOKINGS (last 10)                                            │
│  [Booking ref] [Attendee] [Tickets] [Time]                            │
│                                                                       │
│  [View All Events →]   [Export CSV →]                                 │
└───────────────────────────────────────────────────────────────────────┘
```

**Revenue display note:** Revenue = sum of display prices × quantities. Label clearly: "Indicative Revenue (display prices, not collected by platform)." This removes any ambiguity.

---

### 9.3 Super Admin Dashboard (ADM-01)

```
┌──────────────────────────────────────────────────────────────────────┐
│  Admin Dashboard           Last updated: 06-May-2026 10:30 AM IST   │
├──────────┬──────────┬──────────┬──────────┬──────────┬──────────────┤
│ 👥 Total │ ✅ Active│ 📅 Events│ 🎟 Total │ ⏳ Pending│ 🔄 Re-review │
│ Orgs 87  │ Orgs 64  │ Live: 23 │ Bookings │ Approval │ Queue        │
│          │          │ Total:204│ 1,284    │    [5]   │    [2]       │
└──────────┴──────────┴──────────┴──────────┴──────────┴──────────────┘
│                                                                       │
│  ACTIONS NEEDED                                                       │
│  🔴 5 organizers pending approval (oldest: 4 days)  [Review →]       │
│  🟡 2 profile re-reviews pending                    [Review →]       │
│  🟡 3 reviews flagged for moderation               [Review →]        │
│                                                                       │
│  SYSTEM HEALTH                                  ● All systems OK     │
│  DB: ✅  SMTP: ✅  Scheduler: ✅  Storage: ✅                          │
│                                                                       │
│  RECENT ACTIVITY (Audit Log — last 20)                               │
│  [timestamp] [actor] [action] [entity]                               │
└───────────────────────────────────────────────────────────────────────┘
```

---

## 10. Form Field UX Recommendations

### 10.1 General Rules

| Rule | Implementation |
|------|---------------|
| **Label position** | Always above the field (not placeholder-only). Placeholder is additional hint only. |
| **Required indicator** | Asterisk `*` after label. Helper text below: "* Required fields" once per form. |
| **Inline validation** | Validate on `blur` (not on every keystroke). Exception: password strength meter validates live. |
| **Error messages** | Below the field in red. Specific, not generic. ❌ "Invalid" ✅ "Email must include @ and a domain." |
| **Character counters** | Show when limit ≤ 500 chars. Format: "142 / 3000". Turns amber at 90%, red at 100%. |
| **Success state** | Green border + ✓ checkmark icon on valid blur for high-stakes fields (email, password). |
| **Disabled states** | Clear visual dimming + tooltip explaining why. |
| **Autofocus** | First field of modal/page gets focus on open/load. |

### 10.2 Specific Field Recommendations

| Field | Recommendation |
|-------|---------------|
| **Password** | Show/hide toggle. Real-time strength meter with 5 levels. Requirements list: 8+ chars, upper, lower, number, special. Requirements check off individually as met. |
| **Phone** | Always prefixed with `+91` (fixed, non-editable). Accepts 10 digits. Auto-format: `98765 43210`. |
| **Date/Time** | Use Angular Material datepicker + timepicker. Display in IST. Store UTC. Show "IST" label next to time. |
| **Price (₹)** | `₹` prefix inline in input. Numeric only. Placeholder: `0` (free). Never show payment language. |
| **Coupon code** | UPPERCASE auto-conversion. Trim whitespace on blur. Inline [Apply] button adjacent (not below). |
| **Rich text** | Angular editor (e.g. Quill). Toolbar: Bold, Italic, UL, OL, Link. No images in description. Character limit counter visible. |
| **File upload** | Drag-and-drop zone + click fallback. Show filename + size after upload. Show progress bar. Reject on wrong type/size with specific message: "File must be JPG or PNG under 2 MB." |
| **Quantity stepper** | [−] count [+] with large tap targets (min 44px). Never go below 0. |
| **T&C checkbox** | Checkbox + linked text. Label must include link: "I accept the [Terms & Conditions]". Cannot be pre-checked. |
| **Search** | Debounce 300ms. Show spinner while searching. "No results" state with suggestion. |
| **Reason textarea (admin)** | Mandatory for rejection/suspension. Min 20 chars enforced. Counter shown. |

### 10.3 Error State Hierarchy

```
1. Prevent   — disable submit until form valid
2. Warn      — inline field error on blur
3. Confirm   — modal confirmation for destructive actions
4. Recover   — clear, actionable toast with CTA ("Try again", "Edit booking")
5. Escalate  — full-page error for unrecoverable states (session expired, etc.)
```

---

## 11. Responsive & Mobile Behavior

### 11.1 Breakpoint Behavior Summary

| Component | Mobile (< 600px) | Tablet (600–960px) | Desktop (960px+) |
|-----------|-----------------|-------------------|-----------------|
| Event cards grid | 1 column | 2 columns | 4 columns |
| Event detail layout | Stacked (banner → info → sticky footer CTA) | 2-col: banner+info left, booking card right | Same as tablet, max-width 1200px |
| Booking flow | Full-screen steps | Centred card 600px | Centred card 700px |
| Organizer side nav | Hidden, hamburger → drawer | Collapsible icon rail | Full sidebar 240px |
| Admin side nav | Hidden, hamburger → drawer | Collapsible icon rail | Full sidebar 240px |
| Dashboard KPI cards | 2×2 grid | 3-wide | 6-wide |
| Event creation wizard | Steps: top horizontal scroll | Same | Same |
| Check-in scanner | Full viewport (critical) | Full viewport | Centred 600px max |
| Tables (bookings, events) | Cards layout (each row becomes card) | Table with horizontal scroll | Full table |
| Modals | Bottom sheet (slides up) | Centred modal | Centred modal |

### 11.2 Mobile-Specific Rules

- **Touch targets:** Minimum 44×44px for all interactive elements.
- **Bottom navigation:** Attendee portal uses bottom tab bar on mobile (Explore / My Tickets / Profile).
- **Sticky booking CTA:** On event detail page mobile, booking card collapses to sticky bar at bottom: "From ₹500 · [Book Now]".
- **Scanner viewport:** Camera viewfinder must be full-screen on mobile. No scrolling on scanner page.
- **Form scrolling:** Long forms scroll naturally. Sticky submit button at bottom of viewport.
- **Keyboard avoidance:** When keyboard opens, form fields scroll to remain visible above keyboard.
- **Landscape orientation:** Scanner and confirmation page should handle landscape gracefully. Booking flow — landscape allowed but portrait recommended.
- **No horizontal scroll:** Never allow horizontal overflow on any screen.

### 11.3 Progressive Enhancement for Check-in on Mobile

Since check-in staff use their phones at the venue:
1. **First visit:** Show "Add to Home Screen" prompt (PWA install suggestion).
2. **Camera permission:** Request once with clear explanation: "We need camera access to scan QR codes."
3. **Low-light:** Scanner should work in low-light conditions. Note to FE: use html5-qrcode's `facingMode: environment` + adequate `qrbox` size.
4. **Grace under WiFi failure:** If API call fails during scan, show: "Connection issue. Try manual lookup." Never crash or freeze.

---

## 12. UX Consistency Guidelines for Angular Developers

### 12.1 Component Usage Standards

| Pattern | Use This | Not This |
|---------|----------|----------|
| Primary action | `mat-raised-button color="primary"` | Custom-styled div |
| Danger action | `mat-stroked-button color="warn"` | Red text link |
| Icon buttons | `mat-icon-button` with tooltip | Bare `<button>` with emoji |
| Tables | `mat-table` with pagination | HTML `<table>` |
| Dialogs / modals | `MatDialog` service | CSS overlay |
| Snackbar / toast | `MatSnackBar` | Alert() or custom toast |
| Date pickers | `mat-datepicker` | `<input type="date">` |
| Progress | `mat-progress-bar` | Custom loader |
| Chips / badges | `mat-chip` | Styled spans |
| Expansion panels | `mat-expansion-panel` | Collapsed divs |

### 12.2 State Management Conventions

| State | Visual Treatment |
|-------|----------------|
| Loading | `mat-spinner` centred OR `mat-progress-bar` at top of card |
| Empty state | Illustration + heading + optional CTA. Never just "No data." |
| Error state | Alert card in red with specific message + retry CTA |
| Success | `MatSnackBar` with green panel class for 3 seconds (non-blocking) |
| Destructive confirm | Always `MatDialog` with event title + consequence + Cancel/Confirm pair |

### 12.3 Angular Route Guard Requirements

| Route | Guard |
|-------|-------|
| `/my/**` | `AuthGuard` — redirect to `/login?returnUrl=...` |
| `/organizer/**` | `OrganizerGuard` — role = Organizer + status = Active |
| `/organizer/**` (when status = Profile Update Pending Review) | Allow read; block publish actions inline |
| `/admin/**` | `AdminGuard` — role = SuperAdmin |
| `/checkin/**` | `CheckinStaffGuard` — role = CheckinStaff |
| `/events/:slug/book` | `AuthGuard` — redirect to login |

### 12.4 Date & Currency Formatting

```typescript
// Date pipe — always IST display
{{ event.startDate | date: 'dd-MMM-yyyy hh:mm a' }} IST

// Currency — Indian comma format
{{ amount | currency: 'INR' : 'symbol' : '1.0-0' }}
// Output: ₹1,00,000

// Never show: $, USD, or European comma format for currency
```

### 12.5 Loading & Optimistic UI Rules

| Action | Strategy |
|--------|----------|
| Browse/search | Skeleton loaders on card placeholders while API resolves |
| Booking confirm | Spinner on button, disable button, await full response (no optimistic) |
| Scan QR | Show result immediately from API response — no optimistic |
| Dashboard KPIs | Skeleton cards; replace with real data |
| Form submit | Spinner on button; keep form fields enabled until error or success |

### 12.6 Accessibility Baseline

- All form fields: `aria-label` or `aria-labelledby`.
- Scan outcomes: `aria-live="assertive"` on result panel (screen-reader announcement).
- Color alone never conveys state — always paired with icon + text.
- Focus trap inside modals and bottom sheets.
- Skip navigation link at top of every page.
- Tab order follows visual flow (no `tabindex > 0` shortcuts).
- Minimum contrast ratio: 4.5:1 for body text, 3:1 for large text.

### 12.7 Error & Notification Patterns for Angular

```
Inline form error:    mat-error inside mat-form-field (Angular Material native)
API error (non-fatal): MatSnackBar, red panel, 5 seconds, [Retry] action
API error (fatal):     Full-page error component with [Go Home] CTA
Session expiry:        Intercept 401 → toast "Session expired. Please log in." → redirect /login
Network offline:       Global offline banner at top (use navigator.onLine + event listeners)
```

### 12.8 Naming Conventions for Routes & Feature Modules

```
Feature modules (lazy-loaded):
  - PublicModule       → routes: /, /events/**, /login, /register/**
  - AttendeeModule     → routes: /my/**
  - OrganizerModule    → routes: /organizer/**
  - AdminModule        → routes: /admin/**
  - CheckinModule      → routes: /checkin/**

Shared components (in SharedModule):
  - EventCardComponent
  - BookingStatusBadgeComponent
  - QrDownloadButtonComponent
  - ConfirmDialogComponent
  - InlineAlertComponent
  - SkeletonLoaderComponent
  - CurrencyDisplayComponent (₹ format)
  - DateTimeDisplayComponent (IST format)
```

### 12.9 Key UX Invariants (Non-Negotiable)

1. **Booking confirmation page must always be reachable by its URL** — even on page refresh. Store booking ref in route; never only in memory.
2. **QR download must work without re-authentication** — signed URL or session-scoped endpoint.
3. **Scanner never blocks itself** — after each outcome, auto-resume. No tap needed to continue scanning.
4. **Cancelled event / booking must be visually unmistakable** — red banner, not just a status badge buried in text.
5. **Online meeting link is never shown publicly** — server-enforced; FE never renders it for non-confirmed attendees.
6. **Price never implies payment will be collected** — label as "Display Price" in all organizer-facing forms; no checkout-style language.
7. **Destructive actions always require explicit confirmation** — two steps, always.
8. **All timestamps are IST** — stored UTC, displayed IST. Every single date on the platform.

---

## Appendix A — Screen Count Summary

| Portal | Screens |
|--------|---------|
| Public / Guest | 14 |
| Attendee | 11 |
| Organizer | 15 |
| Check-in Staff | 5 |
| Super Admin | 15 |
| **Total** | **60** |

## Appendix B — Critical UX Flows Summary

| Flow | Steps | Key UX Risk | Mitigation |
|------|-------|-------------|-----------|
| Attendee Booking | 3 steps | Login wall surprise | Return URL preserved |
| Organizer Event Publish | 4 steps | Pre-publish failure at step 4 | Live validation per step |
| QR Check-in | Scan → outcome → resume | Slow camera focus | Large viewfinder, auto-resume |
| Admin Org Approval | Queue → Detail → Decision | Missing doc context | Inline doc viewer |
| Attendee Cancellation | Booking detail → Confirm modal | Accidental cancel | Two-step modal with consequence text |

---

*UX specification produced by Sally. Ready for architecture handoff to Winston and implementation by Amelia. Run each in a fresh context referencing this document.*
