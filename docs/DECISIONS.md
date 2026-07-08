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

---

_Add new decisions below as they come up (e.g., JWT storage on the frontend, deployment
platform choice, pagination strategy)._
