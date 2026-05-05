# PM-06 — Delivery Risks & PM Recommendations

**Project:** Smart Event Management & Ticket Booking Platform
**Owner:** John (PM)
**Date:** 2026-05-05

---

## 1. Risk Register

Scoring: **L** (Likelihood 1-5) × **I** (Impact 1-5) = **R** (Risk Score). Anything ≥ 12 needs an owner + active mitigation.

### Tier 1 — Critical Risks (R ≥ 12)

| ID | Risk | L | I | R | Owner | Mitigation |
|----|------|---|---|---|-------|-----------|
| **DR-01** | **No payment gateway in V1 → organizer rejection** | 4 | 5 | **20** | PM | Position V1 as free pilot; sign first 5 organizers as pilot partners with explicit Phase 2 commitment; "Reserved/Pay-Later" clearly marked |
| **DR-02** | **Email deliverability — booking confirmations land in spam** | 4 | 5 | **20** | BE Lead | Set up SPF/DKIM/DMARC on day 1 of S1; use reputable SMTP (Brevo / SES / Sendinblue); monitor bounce rate; send confirmations from a domain matching the platform |
| **DR-03** | **Atomic inventory implementation bug → overselling** | 3 | 5 | **15** | Architect | Architect signs off on locking pattern before S5; load test in S5 with 100 concurrent confirmations on same category; explicit overselling test in CI |
| **DR-04** | **Underestimated team velocity → MVP slips** | 4 | 4 | **16** | PM | Re-baseline after S2; cut Should-Haves to V1.1 aggressively; shrink scope before extending timeline |
| **DR-05** | **QR HMAC secret leak / loss** | 2 | 5 | **10 → 12 weighted** | Backend Lead | Secret in env var only, rotated yearly; key-id in token enables rotation; document rotation procedure pre-GA |
| **DR-06** | **Browser scanner unreliable on cheap Android phones at venue** | 4 | 4 | **16** | Frontend Lead | Use html5-qrcode (proven library); test on minimum-spec devices in S6; add manual lookup as documented fallback; in V1.1, add bulk attendee CSV download for offline day |

### Tier 2 — Important Risks (R 6–11)

| ID | Risk | L | I | R | Mitigation |
|----|------|---|---|---|-----------|
| DR-07 | Single Super Admin = bottleneck for organizer approvals | 3 | 3 | 9 | Document SLA target 48h; build admin dashboard with sortable queue from S2; V1.1 introduces sub-roles |
| DR-08 | Local file storage fills VPS disk | 3 | 3 | 9 | Monitor disk usage; cap upload sizes strictly (NFR-1001..1003); document migration to S3-compatible storage as Phase 2 |
| DR-09 | VPS provider outage | 2 | 4 | 8 | Pick provider with documented SLA; off-host backup; runbook for failover; statuspage.io free tier |
| DR-10 | Privacy Policy / T&C content not delivered before launch | 3 | 3 | 9 | PM follow-up weekly with stakeholder starting S6; placeholders allowed in beta but BLOCK GA |
| DR-11 | Hangfire / scheduler misfires (missed reminders, missed auto-completion) | 3 | 3 | 9 | Hangfire dashboard exposed to admin; daily monitoring during beta; idempotent jobs only |
| DR-12 | Coupon abuse (mass account creation to redeem) | 3 | 3 | 9 | Enforce per-user limit + minimum booking amount; rate-limit coupon validation API; phone number captured for fraud detection |
| DR-13 | Organizer publishes inappropriate event content | 3 | 3 | 9 | Admin can close event with cause; T&C clearly forbids; reactive moderation acceptable in V1 |
| DR-14 | Time zone bugs (server UTC vs IST display) | 3 | 3 | 9 | Backend uses DateTimeOffset; explicit UTC storage; UI converts; CI test on date-boundary edge cases |
| DR-15 | "Reserved/Pay-Later" creates phantom inventory hoarding | 3 | 4 | 12 | Add per-user active-reservation cap (PM challenge in PM-01 §3); track no-show rate per organizer |

### Tier 3 — Watch List (R ≤ 6)

| ID | Risk | Mitigation |
|----|------|-----------|
| DR-16 | Organizer abandons mid-onboarding | Track funnel; one nudge email after 3 days |
| DR-17 | Attendee password fatigue | "Remember Me" + Phase 2 SSO |
| DR-18 | Browser back button creates duplicate bookings | Idempotency-key on POST /bookings/confirm |
| DR-19 | CSV exports for events with 10K+ tickets blow memory | Stream CSV; cap at 50K rows |
| DR-20 | Reviews used to defame organizers | Admin remove + 500-char limit + checked-in-only gate |

---

## 2. Open Decisions Pending

| # | Decision | Needed By | Recommendation |
|---|----------|-----------|----------------|
| OD-01 | SMTP provider final choice | Pre-S1 | Brevo (free tier 300/day → cheap upgrade) OR Amazon SES (cheap at scale) |
| OD-02 | VPS provider | Pre-S1 | Hetzner CX22 (cheap, EU); for India proximity → AWS Mumbai Lightsail or DigitalOcean Bangalore |
| OD-03 | Domain name + SSL | Pre-S1 | Stakeholder owns this |
| OD-04 | Privacy Policy + T&C content | Pre-Beta | Stakeholder must source |
| OD-05 | First 5 seed organizers | Pre-S6 | PM identifies via outreach by start of S6 |
| OD-06 | JWT signing algorithm | Architect call (pre-S1) | Recommend HS256 for V1 (single API); RS256 only if Phase 2 splits services |

---

## 3. Assumptions Tracker

| ID | Assumption | If wrong... |
|----|------------|-------------|
| A-01 | Team = 1 BE + 1 FE + 0.5 QA | If solo dev → double sprint count |
| A-02 | Stakeholder available for weekly demos | If not → slower feedback loop, hidden defects |
| A-03 | SMTP deliverability achievable on shoestring | If not → rethink email strategy or budget paid SMS in Phase 2 sooner |
| A-04 | Architect (Winston) joins before S1 starts | If not → S1 partly blocked on stack scaffolding |
| A-05 | Designer (Sally) provides wireframes for booking + check-in by S4 | If not → FE designs ad-hoc; UX risk |

---

## 4. PM Recommendations (Summary)

### Recommendation 1 — Convene a Pre-Sprint-1 Decision Day
Before any code is written, lock the **6 Open Decisions** above in one session with stakeholder + architect + dev lead. Each is a 5-minute decision; collectively they unblock 4 weeks of work.

### Recommendation 2 — Run a "Friendly Organizer" Pilot
Identify 2 friendly organizers willing to test rough UI in **Sprint 4** (after event publish works). Their feedback in Sprints 5–6 is worth more than any spec doc.

### Recommendation 3 — Aggressively Defer Should-Haves
The MVP cuts in PM-01 are real. Don't let scope creep back in mid-sprint. Anyone proposing "while we're in there" → automatic V1.1 discussion.

### Recommendation 4 — Demo Weekly, Even if Ugly
End of every sprint, 30-minute demo to stakeholder + at least 1 outside person (friend, parent, anyone non-technical). Catches UX blind spots cheaply.

### Recommendation 5 — Velocity-Driven Re-planning
After Sprint 2, recalculate sprint count. Don't pretend Sprint 1 estimates were right. Communicate honestly to stakeholder; share the recalibrated date.

### Recommendation 6 — Build Audit Log Early
Audit infrastructure (US-E2-010) is a Sprint 2 story. Once it exists, every subsequent module writes to it. **Don't skip this.** It's the cheapest insurance against compliance/dispute pain.

### Recommendation 7 — Write the Runbook in Parallel
Don't leave the production deployment runbook for the last day. Update it incrementally each sprint as new infra goes in (DB, Hangfire, file storage, SMTP). By Sprint 8 it should already be 90% done.

### Recommendation 8 — Define "Done" Per Story Strictly
Each story is "done" when:
1. Code merged to main
2. Unit tests added
3. Acceptance criteria from Mary's [07](../planning-artifacts/07-acceptance-criteria.md) verified manually or automated
4. Deployed to staging
5. PM-walked-through

Anything less is "in progress" — no shipping half-done stories on Friday.

### Recommendation 9 — Reserve 20% Buffer
Never plan a sprint at 100% capacity. 20% buffer absorbs:
- Bug fixes from prior sprint
- Production incidents
- Better-than-expected feedback requiring small pivots
- Holidays / sick days

### Recommendation 10 — Plan the Cut-Over to V2 Now
The biggest single risk to **product success** isn't V1 quality — it's the V1 → V2 jump (adding payments). Architect must design with payment hooks in V1 (already in NFRs and DB schema reserved columns). PM tracks this as a non-functional acceptance criterion: *"V2 payment integration requires zero schema migration on existing tables."*

---

## 5. Go / No-Go Decision Framework

**At each release gate** (Alpha, Closed Beta, Open Beta, GA), apply:

✅ **GO** if:
- All P0 acceptance criteria pass
- Zero critical security findings
- Performance NFRs met under expected load
- Backup + restore drill successful
- Stakeholder formally signs off

🟡 **CONDITIONAL GO** if:
- 1–2 P1 issues with documented workaround
- Performance within 20% of target (improve in next sprint)

🛑 **NO-GO** if:
- Any P0 acceptance criterion fails
- Any data-loss / overselling / security incident in last 7 days
- Backup/restore drill failed
- Stakeholder withholds sign-off

---

## 6. Communication Cadence

| Cadence | Audience | Format |
|---------|----------|--------|
| Daily (sprint) | Dev team | 15-min standup |
| Weekly | Stakeholder | Status email: shipped / in-flight / blocked / risks |
| Sprint end (every 2 weeks) | Stakeholder + extended team | Demo + retrospective |
| Monthly | Wider audience (e.g., advisors) | One-page summary: north-star metric + key risk |
| Real-time | Internal dev | #incidents Slack channel |

---

## 7. Final PM Verdict on Project Health

🟢 **Green for build kickoff** with these conditions:
1. Architect on board before Sprint 1 (Open Decision OD-06)
2. SMTP + VPS chosen before Sprint 1 (OD-01, OD-02)
3. Stakeholder commits to weekly demo cadence
4. Privacy/T&C content pipeline started in Sprint 4 (Open Decision OD-04)
5. PM owns the seed-organizer pipeline starting Sprint 4 (Open Decision OD-05)

Without any of (1) (2) (3) — **delay coding until resolved.** The cost of a 1-week delay is far less than the cost of restarting a sprint.

---

**End of Delivery Risks & PM Recommendations**
