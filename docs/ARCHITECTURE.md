# Architecture

A one-page map of how the pieces fit together. For *why* a given piece is built the way
it is, see [DECISIONS.md](DECISIONS.md) — this file covers structure, not rationale.

## Layered backend

```
Controller  →  Service  →  AppDbContext (EF Core)  →  PostgreSQL
   (HTTP)      (business rules)   (data access)
```

- **Controllers** (`Controllers/`) only translate HTTP ↔ DTOs and call one service
  method. No business logic lives here — every action reads as "get the current user,
  call the service, map the result to a status code."
- **Services** (`Services/Implementations/`, behind interfaces in `Services/Interfaces/`)
  own every business rule: ownership checks, the internship publishing workflow,
  duplicate-application prevention, status-transition restrictions, company approval
  gating. Each public method returns an `OperationResult`
  (`Success`/`NotFound`/`Forbidden`/`ValidationFailed`) so the controller can map
  outcomes to HTTP status codes without re-deciding the business rule itself.
- **`AppDbContext`** (`Data/AppDbContext.cs`) is EF Core talking directly to Postgres —
  no repository layer sits between services and the DbContext (a deliberate simplicity
  choice; see DECISIONS.md D1-era reasoning). Migrations under `Data/Migrations/`
  apply automatically at startup (`Program.cs`), in every environment.
- **DTOs** (`DTOs/`) define every request/response shape, kept separate from
  **Entities** (`Entities/`), which map 1:1 to tables — a request never binds directly
  to an entity.

## Request lifecycle example — a student applying to an internship

```
POST /api/internships/{id}/apply
        │
        ▼
[Authorize(Roles="Student")]  →  401/403 if not a logged-in Student
        │
        ▼
InternshipsController.Apply
        │  reads the caller's user id from the JWT (ICurrentUserAccessor)
        ▼
ApplicationService.ApplyAsync
        │  is the post Open? deadline passed? already applied?
        ▼
AppDbContext  →  INSERT, protected by a unique (StudentId, InternshipPostId) index
        │
        ▼
201 Created — ApplicationDto,  or  400 with an RFC 9457 Problem Details body
```

Every error response in the API — validation failures, business-rule rejections,
`[Authorize]` failures, and unhandled exceptions alike — uses the same
[RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) shape (D17), so the
frontend has exactly one error-handling code path (`src/api/client.ts`) for the entire API.

## Authentication

Custom JWT, not ASP.NET Core Identity (a deliberate choice — see D2-era reasoning in
DECISIONS.md): register/login hash the password with BCrypt and hand back a signed JWT
carrying the user's id, email, and role as claims; `[Authorize(Roles = "...")]` reads
the role claim directly, no extra configuration needed. Tokens are stateless — nothing
server-side tracks "logged in" sessions, which is simple but means disabling a user
blocks *future* logins without revoking an already-issued token (documented,
deliberate limitation — see D16).

## Frontend

```
Pages (one per route)  →  api/*.ts (one file per REST resource)  →  client.ts (fetch)
        │
        ▼
AuthContext (who's logged in) + ProtectedRoute (route guards)
```

- **`src/api/`** — one thin module per backend resource (`auth.ts`, `internships.ts`,
  `applications.ts`, `students.ts`, `companies.ts`, `admin.ts`), each just a typed
  wrapper around `apiRequest<T>()` in `client.ts`. No axios, no generated client — the
  API surface is small enough that hand-written stays the most readable option (D18).
- **`src/context/AuthContext.tsx`** — the one piece of state multiple unrelated
  components need (who's logged in), backed by `localStorage`.
- **`src/components/ProtectedRoute.tsx`** — a route wrapper, not a framework feature:
  not authenticated → redirect to `/login` (remembering where you were headed);
  wrong role → redirect to the public listing (D19).
- **`src/pages/`** — one component per route, organized by role
  (`student/`, `company/`, `admin/`) once past the shared public pages.

## Project layout

```
backend/
  src/InternshipManagement.Api/   the API itself (Controllers, Services, Entities, DTOs, Data)
  tests/InternshipManagement.Tests/  xUnit tests, one file per service, against an
                                      in-memory EF Core database (D20)
frontend/
  src/
    api/          one module per backend resource
    components/   shared UI (Navbar, ProtectedRoute, ErrorMessage)
    context/      AuthContext
    pages/        one component per route
    types/        hand-mirrored backend DTOs/enums
docs/              every phase's documentation - see PHASES.md for the roadmap this
                   project was built against, DECISIONS.md for why things are the way
                   they are, and phase-summaries/ for a full build log
docker-compose.yml + render.yaml   local and public deployment topologies (D21/D22)
```

## Deployment topologies

Two different topologies exist for two different situations, and the difference is
worth understanding (see D21/D22 for the full reasoning):

- **Docker Compose** (local): nginx serves the frontend *and* reverse-proxies `/api/*`
  to the backend container — same origin, so the browser never needs CORS.
- **Render** (public): frontend and backend are two independently-deployed services on
  two different domains — CORS bridges them, configured via an environment variable
  rather than hardcoded.
