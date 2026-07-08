# Project Phases

This is the execution roadmap. The project is built **one phase at a time**: implement →
explain → test against the acceptance criteria → commit → next. Each phase produces a
result you can verify on its own.

**Verification styles:** _Runtime_ = run it and hit it via Swagger/browser · _Auto_ =
`dotnet test` · _Review_ = an artifact/checklist you can explain out loud.

**Difficulty:** ⭐ (trivial) … ⭐⭐⭐⭐⭐ (hard for a beginner).

| # | Phase | Objective / key deliverable | Verify | Difficulty | Est. |
|---|---|---|---|---|---|
| 0 | Project Scope & Requirements | Scope, requirements, and docs before any code | Review | ⭐ | 1–2h |
| 1 | Environment & Backend Skeleton | Install toolchain; empty API running with Swagger | Runtime | ⭐⭐ | 2–4h |
| 2 | Just-Enough C# | Learn only the C# used later (classes, enums, interfaces, async) | Review | ⭐⭐ | 3–5h |
| 3 | Database Design (on paper) | Entities, relationships, constraints, ERD | Review | ⭐⭐ | 3–4h |
| 4 | EF Core Setup & First Migration | Real PostgreSQL tables from C# entities | Runtime + DB | ⭐⭐⭐ | 4–6h |
| 5 | Internship CRUD API (no auth) | Full CRUD driven from Swagger | Runtime | ⭐⭐⭐ | 5–7h |
| 6 | Authentication & Roles (custom JWT) | Register/login + role-based access | Runtime | ⭐⭐⭐⭐ | 6–9h |
| 7 | Student & Company Profiles | Each user manages own profile; company approval flag | Runtime | ⭐⭐⭐ | 5–7h |
| 8 | Internship Publishing Workflow | Draft→Open→Closed, owned by logged-in company | Runtime | ⭐⭐⭐⭐ | 6–8h |
| 9 | Student Application Workflow | Apply / track / withdraw with core rules | Runtime | ⭐⭐⭐⭐ | 6–9h |
| 10 | Company Application Review | Company reviews own applicants; set statuses | Runtime | ⭐⭐⭐⭐ | 5–7h |
| 11 | Admin Dashboard & Management | Stats, approvals, user management | Runtime | ⭐⭐⭐ | 5–7h |
| 12 | Validation, Errors & API Quality | Validation, global errors, pagination/filter/search | Runtime | ⭐⭐⭐ | 5–7h |
| — | **Milestone A** | Backend complete & demoable end-to-end via Swagger | Runtime | — | — |
| 13 | React Frontend Basics | Login + internship listing + apply, in the browser | Runtime | ⭐⭐⭐⭐ | 8–12h |
| 14 | Role-Based Dashboards | Student/Company/Admin dashboards, protected routes | Runtime | ⭐⭐⭐⭐ | 10–14h |
| — | **Milestone B** | Full-stack app demoable in the browser | Runtime | — | — |
| 15 | Testing | xUnit tests for core business rules | Auto | ⭐⭐⭐ | 6–9h |
| 16 | Docker & Local Prod Simulation | backend + frontend + Postgres via one command | Runtime | ⭐⭐⭐⭐ | 8–12h |
| 17 | Deployment | Public live demo (Render/Railway) | Runtime | ⭐⭐⭐⭐ | 6–10h |
| 18 | Final Docs, Portfolio & CV | README, screenshots, CV bullets, interview prep | Review | ⭐⭐ | 4–6h |

**Total:** ~99–145 hours of focused work (beginner pace; expect extra reading time).

## End-to-End Smoke Test

Run after Phase 12 (Swagger) and again after Phases 14 / 16 / 17 (UI / live):

1. Company registers → login.
2. Admin approves the company.
3. Company posts an internship (Draft) → opens it.
4. Student registers → login → sees the open post.
5. Student applies → appears as `Pending` in "my applications".
6. Student tries to apply again → rejected (no duplicates).
7. Company reviews → sets `Accepted` → student sees the updated status.
8. Admin dashboard counts reflect all of the above.

If all 8 steps pass, the system works end-to-end.
