# Decision Log

Lightweight ADR (Architecture Decision Record) log. Each entry captures a decision, the
reason, and the alternative we rejected — so the choices can be explained in an interview.

---

## D1 — Database: PostgreSQL
- **Decision:** Use PostgreSQL as the database.
- **Why:** Free, cross-platform, Docker-friendly, and supported on free deployment tiers
  (Render/Railway) — the best long-term fit for a portfolio project meant to go live.
- **Rejected:** SQL Server LocalDB (easiest on Windows, but harder/costlier to deploy for
  free and less Docker-friendly).

## D2 — Authentication: custom lightweight JWT
- **Decision:** Build a custom `User` entity + BCrypt password hashing + manual JWT
  generation, rather than ASP.NET Core Identity.
- **Why:** Full transparency. Every step (hashing, token creation, role claims, validation)
  is written by us, so it can be fully understood and explained in an interview — matching
  the learning-first goal.
- **Rejected:** ASP.NET Core Identity (industry-standard and resume-worthy, but more
  "magic" and a steeper learning curve for a beginner).

## D3 — Backend stack: ASP.NET Core Web API + EF Core (Npgsql)
- **Decision:** ASP.NET Core Web API on the current .NET LTS SDK, with Entity Framework
  Core using the Npgsql PostgreSQL provider.
- **Why:** Matches the project brief and the CV target (C#/.NET). EF Core gives a clean,
  code-first path from C# entities to database tables via migrations.
- **Rejected:** Dapper / raw SQL (more control, but more boilerplate and less beginner-friendly).

## D4 — Frontend stack: React + TypeScript via Vite
- **Decision:** React with TypeScript, scaffolded with Vite.
- **Why:** Vite is the modern standard with fast dev tooling; TypeScript adds type safety
  that mirrors the backend DTOs.
- **Rejected:** Create-React-App (deprecated / unmaintained).

## D5 — Architecture: simple layered (Controllers → Services → EF Core)
- **Decision:** Thin controllers, business logic in services, data access via a single
  `AppDbContext`. No repository pattern, CQRS, or MediatR.
- **Why:** Keeps the codebase understandable for a beginner while still teaching separation
  of concerns. Avoids over-engineering (an explicit project rule).

## D6 — Build cadence: one phase at a time with a test gate
- **Decision:** Implement one phase, explain it, verify it against acceptance criteria,
  commit, then move on.
- **Why:** Maximizes understanding and makes every phase independently testable — the core
  requirement of this project.

## D7 — Swagger UI via Swashbuckle.AspNetCore (not the built-in OpenApi package)
- **Decision:** Use `Swashbuckle.AspNetCore` for API documentation instead of the .NET 10
  template default (`Microsoft.AspNetCore.OpenApi`).
- **Why:** The template default only serves the raw OpenAPI JSON — no browsable UI. The
  project brief and phase acceptance criteria require an interactive Swagger page to
  manually test endpoints, which is exactly what Swashbuckle's `/swagger` UI provides.
  As a side effect, it also resolved a NuGet security advisory (NU1903) on the transitive
  `Microsoft.OpenApi` 2.0.0 dependency pulled in by the template default.
- **Rejected:** Keeping `Microsoft.AspNetCore.OpenApi` + adding a separate UI (e.g. Scalar)
  — more moving parts for no benefit over Swashbuckle's all-in-one package.

## D8 — Primary keys: `int` identity, not `Guid`
- **Decision:** Every table uses an auto-increment `int Id` as its primary key.
- **Why:** Simpler to read, type, and debug while testing in Swagger/Postman
  (`GET /internships/3` vs. a long GUID string). This project has no distributed/offline
  data-merging scenario that would require globally unique IDs.
- **Rejected:** `Guid` primary keys (avoids exposing sequential IDs and is common with
  ASP.NET Identity, but adds unnecessary friction for a beginner project with no need for
  ID unguessability).

## D9 — `User` is separate from `StudentProfile`/`CompanyProfile`; no `AdminProfile`
- **Decision:** Authentication data (`Email`, `PasswordHash`, `Role`) lives in one `User`
  table. Role-specific data lives in separate `StudentProfile`/`CompanyProfile` tables,
  linked 1–1 by a unique `UserId` foreign key. Admins are just a `User` with
  `Role = Admin` — there is no `AdminProfile` table.
- **Why:** Avoids one wide table full of nulls (a company row would have no `FullName`, a
  student row would have no `CompanyName`). Keeps auth concerns isolated from profile
  concerns. Admins have no extra fields in `docs/REQUIREMENTS.md`, so a profile table for
  them would be an empty, unused table.
- **Rejected:** One single `User` table with every possible field for every role
  (simpler at first glance, but leads to a sparse, confusing schema as fields grow).

## D10 — `Skills` stored as one comma-separated string, not a normalized table
- **Decision:** `StudentProfile.Skills` is a single free-text string column.
- **Why:** A fully normalized `Skill` + `StudentSkill` many-to-many table is the "more
  correct" relational design, but it's unnecessary complexity for v1 — nothing in the MVP
  requirements needs to query/filter by individual skill. Matches the project rule to
  avoid over-engineering.
- **Rejected:** Normalized `Skill`/`StudentSkill` tables (logged as a candidate under
  README "Future Improvements" if skill-based search is ever needed).

## D11 — Local DB credentials live in .NET User Secrets, not in a committed file
- **Decision:** The real PostgreSQL connection string (with the password) is stored via
  `dotnet user-secrets`, which writes it to a per-user file outside the repository
  entirely (`%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`). `appsettings.Development.json`
  only holds a placeholder connection string with an obviously-fake password
  (`set-via-dotnet-user-secrets`), which fails fast with a clear auth error if secrets
  aren't configured — rather than silently trying to connect with an empty/wrong password.
- **Why:** `appsettings.Development.json` was already committed to git in Phase 1 (it's
  part of the standard template output). Writing a real password into it risks that
  password landing in GitHub history the moment anyone runs a plain `git add`/`commit`.
  User Secrets is the ASP.NET Core-official mechanism for exactly this problem, and it's
  loaded automatically in the `Development` environment with zero extra code.
- **Rejected:** Adding `appsettings.Development.json` to `.gitignore` and putting the real
  password directly in it (works, but is easy to accidentally reverse — e.g., a future
  `git add -A` on a re-created default file — whereas User Secrets can never be committed
  because it never lives inside the project folder at all).
- **Also decided alongside this:** a dedicated, least-privilege PostgreSQL role
  (`internship_app`) and database (`internship_management`) were created for this project
  instead of using the `postgres` superuser — the superuser's password is never used by,
  or known to, this application at all.

---