# Internship Management System

> ⚠️ **Work in progress** — built phase by phase for learning and CV-building.
> Current status: **Phase 0 complete** (scope & requirements). See
> [docs/PHASES.md](docs/PHASES.md) for the roadmap.

A full-stack web application where **students** apply for internships, **companies**
publish and manage internship posts, and **admins** approve companies and monitor the
platform.

## Overview

The system models a realistic internship workflow with real business rules (company
approval, application deadlines, no-duplicate-applications, ownership checks) rather than
plain CRUD. It is built as a learning project with a strong emphasis on understanding and
being able to explain every part.

- **Scope & requirements:** [docs/PROJECT_SCOPE.md](docs/PROJECT_SCOPE.md) ·
  [docs/REQUIREMENTS.md](docs/REQUIREMENTS.md)
- **Roadmap:** [docs/PHASES.md](docs/PHASES.md)
- **Key decisions:** [docs/DECISIONS.md](docs/DECISIONS.md)

## Features

- Role-based auth (Student / Company / Admin) with JWT
- Student profiles and Company profiles
- Admin approval of companies before they can publish
- Internship publishing workflow (Draft → Open → Closed / Cancelled)
- Public internship listing with filtering/search
- Student application workflow (apply / track / withdraw)
- Company application review (shortlist / accept / reject)
- Admin dashboard with platform statistics

_(Features are delivered incrementally — see the roadmap for what is built so far.)_

## User Roles

| Role | Can do |
|---|---|
| **Student** | Manage profile, browse open internships, apply, track & withdraw applications |
| **Company** | Manage profile, publish internships (once approved), review applicants |
| **Admin** | Approve/reject companies, view users/applications, disable users, view stats |

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | C#, ASP.NET Core Web API, Entity Framework Core |
| Database | PostgreSQL |
| Auth | Custom JWT + BCrypt password hashing |
| Frontend | React + TypeScript (Vite) |
| Testing | xUnit |
| Tooling | Git, Swagger/OpenAPI, Docker, Docker Compose |

## Architecture

Simple layered architecture: **Controllers** (HTTP only) → **Services** (business rules)
→ **EF Core `AppDbContext`** (data). DTOs define request/response shapes; Entities map to
tables. _(Details added in `docs/ARCHITECTURE.md` during backend phases.)_

## Database Design

_Documented in `docs/DATABASE_DESIGN.md` (Phase 3), including the ERD and the
`(StudentId, InternshipPostId)` unique constraint that prevents duplicate applications._

## API Documentation

_Swagger/OpenAPI is enabled on the backend (Phase 1+); endpoint reference lives in
`docs/API_SPEC.md`._

## Screenshots

_Added in Phases 14 and 17._

## Getting Started

_Setup instructions (prerequisites, running the API and frontend locally) are added in
Phase 1 and expanded as the project grows._

## Running with Docker

_Added in Phase 16 (`docker compose up`)._

## Running Tests

_Added in Phase 15 (`dotnet test`)._

## Environment Variables

_Documented with `.env.example` in Phase 16._

## Main Business Rules

See [docs/REQUIREMENTS.md](docs/REQUIREMENTS.md#4-core-business-rules).

## Deployment

_Added in Phase 17 (live demo link + steps)._

## What I Learned

See the running [docs/LEARNING_LOG.md](docs/LEARNING_LOG.md).

## Future Improvements

Email notifications, CV file upload, advanced search, saved internships, company ratings,
interview scheduling, admin audit logs, analytics dashboard, AI-based recommendations,
real-time notifications — all deliberately deferred beyond Version 1.
