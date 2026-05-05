# Smart Event Management & Ticket Booking Platform — Planning Artifacts Index

**Project:** Smart Event Management & Ticket Booking Platform
**Version:** 1.0 (V1 Scope Baseline)
**Owner:** Thisanth (Stakeholder)
**Prepared by:** Mary — Business Analyst (BMad Method)
**Date:** 2026-05-05
**Status:** Final — Approved for downstream PRD / UX / Architecture phases

---

## Document Catalog

| # | Document | File | Purpose |
|---|----------|------|---------|
| 01 | Project Scope Baseline | [01-project-scope-baseline.md](01-project-scope-baseline.md) | Locked V1 scope, in/out boundaries, phase split |
| 02 | Enterprise Business Requirements (BRD) | [02-business-requirements-document.md](02-business-requirements-document.md) | Business context, goals, stakeholders, capabilities |
| 03 | Functional Requirements Specification (FRS) | [03-functional-requirements-specification.md](03-functional-requirements-specification.md) | Detailed functional requirements per module |
| 04 | Module Breakdown | [04-module-breakdown.md](04-module-breakdown.md) | Logical modules, sub-modules, ownership |
| 05 | UML Use Cases | [05-uml-use-cases.md](05-uml-use-cases.md) | Actors, use cases, PlantUML diagrams |
| 06 | Agile Epics & User Stories | [06-epics-and-user-stories.md](06-epics-and-user-stories.md) | Epic catalog with user stories |
| 07 | Acceptance Criteria | [07-acceptance-criteria.md](07-acceptance-criteria.md) | Story-level acceptance criteria (Gherkin) |
| 08 | Business Rules & Edge Cases | [08-business-rules-edge-cases.md](08-business-rules-edge-cases.md) | Codified rules + exception scenarios |
| 09 | Non-Functional Requirements | [09-non-functional-requirements.md](09-non-functional-requirements.md) | Security, performance, availability, NFRs |
| 10 | Architect Technical Handoff | [10-architect-technical-handoff.md](10-architect-technical-handoff.md) | Inputs for Winston (architect) |
| 11 | Database Entities Overview | [11-database-entities-overview.md](11-database-entities-overview.md) | Suggested entities and relationships |
| 12 | API Module List | [12-api-module-list.md](12-api-module-list.md) | REST API surface organized by module |

---

## Reading Order

1. Start with **01 — Scope Baseline** to understand what's in/out for V1.
2. Read **02 — BRD** for business context.
3. Use **03 — FRS** + **04 — Module Breakdown** as primary functional reference.
4. **05 — UML** + **06 — Epics** + **07 — AC** drive sprint planning.
5. **08 — Rules & Edge Cases** + **09 — NFR** govern QA test design.
6. **10 — Architect Handoff** + **11 — DB Entities** + **12 — API List** feed the architecture phase.

---

## Tech Stack (Locked)

- **Frontend:** Angular
- **Backend:** ASP.NET Core Web API
- **Database:** MySQL
- **Auth:** JWT (with refresh tokens)
- **Email:** SMTP
- **Storage:** Local server filesystem (V1) → cloud-ready (Phase 2)
- **QR:** Free open-source generator/scanner libraries
- **CI/CD:** GitHub Actions
- **Hosting:** Low-cost VPS / Docker

## Geography & Currency

- **Market:** India only
- **Currency:** INR
- **Language:** English (i18n-ready)
- **Timezone:** IST
