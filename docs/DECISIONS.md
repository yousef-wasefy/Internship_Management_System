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

## D12 — JWT signing key also lives in User Secrets (extends D11)
- **Decision:** The JWT signing key (`Jwt:Key`) is a cryptographically random 512-bit
  value, generated once and stored via `dotnet user-secrets` — never in a committed file.
  `appsettings.json` holds the non-secret `Jwt:Issuer`/`Jwt:Audience`/`Jwt:ExpiryMinutes`;
  `appsettings.Development.json` holds only an obviously-fake `Jwt:Key` placeholder.
- **Why:** The signing key is what makes a JWT unforgeable — anyone with this value could
  mint a valid "admin" token for any account. It's exactly the same class of secret as the
  database password (D11), so it gets the same treatment.
- **Rejected:** Hardcoding a key directly in `Program.cs` or `appsettings.json` (extremely
  common in tutorials, but means anyone with repo access could forge tokens).

## D13 — `MapInboundClaims = false` on the JWT bearer handler
- **Decision:** Set `options.MapInboundClaims = false` when configuring
  `AddJwtBearer(...)`.
- **Why:** By default, ASP.NET Core silently remaps short JWT claim names ("sub", "email")
  to long legacy .NET `ClaimTypes` URIs when a token is validated — so code that reads
  back `JwtRegisteredClaimNames.Sub` (the same constant used to *write* the claim) would
  get `null` unless this remapping is disabled. Turning it off keeps claim names exactly
  as `JwtTokenGenerator` wrote them — one less "magic" behavior to explain or debug
  around, which matters for a project whose whole point is transparency.
- **Rejected:** Reading claims via `ClaimTypes.NameIdentifier` instead (works, but means
  the "write" code and "read" code use different-looking claim type constants for the
  same value, which is confusing to trace for a beginner).

## D14 — Seeded Admin account uses a known, documented dev-only password
- **Decision:** `SeedData` creates one Admin user
  (`admin@internship-system.local` / `Admin@12345`, hashed with BCrypt like every other
  account) so there's something to log in as while testing. The password is documented
  in `docs/API_SPEC.md` and this file, in plain sight.
- **Why:** Some account needs to exist to test login-as-admin and (in Phase 11) the admin
  endpoints. A known, documented dev-only credential is simpler than a setup wizard, and
  is standard practice for local seed data.
- **Must change before deployment:** this account must be rotated or removed before any
  real deployment (Phase 17) — flagged here so it isn't forgotten. It is **not** a
  production credential and must never be treated as one.

## D15 — Shared `OperationResult` enum for ownership-checked service methods
- **Decision:** Service methods that act on a specific resource by id and need to
  distinguish "doesn't exist" from "exists but you don't own it" from "exists, you own
  it, but the request breaks a business rule" return a shared
  `Enums.OperationResult` (`Success`/`NotFound`/`Forbidden`/`ValidationFailed`) — for
  `Open`/`Close`, paired with a `(Result, ErrorMessage, Dto)` named tuple so a
  `ValidationFailed` can also carry a human-readable reason. The controller maps each
  case to the matching HTTP status (404/403/400) with a C# `switch` expression.
- **Why:** Phase 5–7 got by with a plain `bool`/nullable return because "not found" was
  the only failure mode. Phase 8 introduced a second, structurally different failure
  (ownership) plus a third with multiple distinct messages (publishing rules) — a plain
  bool can no longer tell the controller which HTTP status to return. A shared enum
  keeps every controller's status-mapping `switch` block looking the same, rather than
  each controller inventing its own ad-hoc signal.
- **Rejected:** Throwing custom exceptions for each failure case (works, but turns
  expected, everyday business outcomes like "not your post" into exception-driven
  control flow, which is harder to read and slower than a normal return value).
  Also rejected: a full `Result<T>`/`OneOf<T>` generic wrapper library (more powerful,
  but more machinery than a 4-case enum needs for a learning project).
- **Expected reuse:** Phase 9/10's application ownership checks ("a company can only
  review applications for its own internships," REQUIREMENTS.md CO-7/CO-8) are expected
  to follow this exact same pattern.

## D16 — Admin actions moved into a dedicated `IAdminService`; "reject" = disable
- **Decision:** `ApproveCompanyAsync` (originally a stub in `ICompanyService`, Phase 7)
  moved into a new `IAdminService` alongside the Phase 11 additions
  (`RejectCompanyAsync`, `GetDashboardAsync`, `GetPendingCompaniesAsync`,
  `GetUsersAsync`, `DisableUserAsync`). `ICompanyService` is now purely
  company-self-service (view/update its own profile) — no admin-only method remains on
  it. Separately: because `CompanyProfile` has only a boolean `IsApproved` (no
  three-state Pending/Approved/Rejected field), **rejecting a company sets
  `IsApproved = false` and disables its `User` account outright**
  (`IsDisabled = true`), so a rejected company can't log back in and re-apply
  indefinitely, and naturally drops off the "pending" list.
- **Why:** Phase 7 explicitly flagged `ApproveAsync` living on `ICompanyService` as a
  temporary stub, anticipating a real `AdminService` once there was enough admin-only
  logic to justify one (see that phase's code comment). Phase 11 is that point — six
  admin actions is enough to warrant its own service, and keeping approve/reject
  together (both are "an admin acting on a company," not "a company acting on itself")
  is a cleaner boundary than splitting them across two services. For reject: adding a
  proper `ApprovalStatus` enum (Pending/Approved/Rejected) to the schema would be the
  "more correct" relational design, but no current requirement needs to distinguish "was
  rejected" from "was disabled for some other reason" — reusing the existing
  `IsDisabled` flag avoids a schema change and migration for a distinction nothing
  currently reads.
- **Rejected:** Adding a new `ApprovalStatus` field/migration just for this (unnecessary
  schema growth for a distinction with no current consumer — matches the project's
  standing "avoid over-engineering" rule). Also rejected: leaving `ApproveAsync` on
  `ICompanyService` and only adding `RejectAsync` there too (would leave admin logic
  scattered across a service that's supposed to be about company self-service).
- **Also discovered this phase (not this decision's main topic, but related):** disabling
  a user blocks future logins but does **not** revoke an already-issued JWT — tokens are
  stateless and validated purely cryptographically, so a disabled user's existing token
  keeps working (accepted by `[Authorize]`) until it naturally expires. Fixed
  `AuthService.GetCurrentUserAsync` to re-check `IsDisabled` (so `/auth/me` at least
  reports honestly), but did **not** build a general token-revocation mechanism (a
  blocklist, or short-lived tokens + refresh) — that's a substantial feature in its own
  right, out of scope for "add a disable endpoint," and documented as a known limitation
  in `docs/API_SPEC.md` rather than silently left unmentioned.

## D17 — Every error response uses RFC 9457 Problem Details
- **Decision:** All error responses across the entire API — DTO validation failures,
  business-rule violations (`Problem(statusCode, detail)` in controllers), `401`/`403`
  from `[Authorize]` role checks, and unhandled `500`s — return the same
  `application/problem+json` shape (`type`/`title`/`status`/`detail`/`errors`/`traceId`).
  Achieved via three pieces: `builder.Services.AddProblemDetails()` (built into ASP.NET
  Core 8+, already gives `[ApiController]`'s automatic DataAnnotations validation this
  shape for free); every controller's previously-inconsistent
  `Conflict(string)`/`Unauthorized(string)`/`BadRequest(new{message})`/bare
  `NotFound()`/`Forbid()` calls replaced with `Problem(statusCode:, detail:)`; and a
  custom `IAuthorizationMiddlewareResultHandler`
  (`ProblemDetailsAuthorizationMiddlewareResultHandler`) so `[Authorize(Roles=...)]`
  rejections — which happen in framework middleware *before* any controller code runs,
  and previously returned a bare empty-bodied `401`/`403` — get the same shape too.
- **Why:** Phases 5–11 each picked a locally-reasonable but *different* error shape
  (a bare string, `{ message: "..." }`, or nothing at all) because no phase's job was to
  look at the API as a whole. Phase 12's explicit goal is a *consistent* API — RFC 9457
  is the standard shape for HTTP API errors, already partially given for free by
  `[ApiController]`'s validation handling, so extending it everywhere (rather than
  inventing a separate custom shape) means the API matches what any developer already
  familiar with ASP.NET Core or REST conventions would expect.
- **Rejected:** Inventing a custom envelope (e.g. `{ success: false, error: "..." }`) -
  more work to hand-roll consistently, and less recognizable than a named standard.
  Also rejected: leaving the `[Authorize]`-rejection gap unfixed (empty-bodied 401/403)
  as "good enough" - found live while testing (see LEARNING_LOG.md), and fixing it was a
  contained, well-scoped addition directly serving this same decision's goal.
- **Also decided alongside this:** `GET /api/internships`'s response shape changed from a
  bare array to `PagedResult<InternshipListDto>` (pagination, D-adjacent to D17 but
  really about the Phase 12 pagination/filtering/search requirement, not error handling)
  — called out explicitly in `docs/API_SPEC.md` as a breaking change to that one endpoint.

## D18 — Frontend stack: React + TypeScript (Vite), hand-written fetch client, Context for auth, no UI framework
- **Decision:** The `frontend/` app (Phase 13) is scaffolded with Vite's `react-ts`
  template, React Router for client-side routing, a small hand-written `fetch` wrapper
  (`src/api/client.ts`) instead of axios or a generated client, React's built-in Context
  API (`AuthContext`) instead of Redux/Zustand for the one piece of shared state (the
  logged-in user), and plain hand-written CSS instead of a component/utility framework
  like Tailwind or MUI. The backend gained one small addition to support it:
  `AddCors`/`UseCors`, scoped to exactly `http://localhost:5173` (the Vite dev server
  origin) with no `AllowCredentials()`, since auth is a Bearer token in a header, not a
  cookie.
- **Why:** `docs/PROJECT_SCOPE.md` calls for a "simple React + TypeScript frontend" —
  every choice here optimizes for that word. The API surface this frontend talks to is
  small (7 endpoints as of Phase 13), so a hand-written `apiRequest<T>()` wrapper stays
  readable end-to-end in one file, while a generated client or axios would add a
  dependency and a layer of indirection to learn for no real benefit at this scale. One
  Context is enough because there is exactly one piece of state multiple unrelated
  components need (who's logged in) — Redux/Zustand solve problems (complex derived
  state, time-travel debugging, cross-slice coordination) this project doesn't have yet.
  Plain CSS keeps the visual layer legible and matches `PROJECT_SCOPE.md`'s explicit
  "Out of scope: complex UI animations."
- **Rejected:** axios (no functionality gained here that `fetch` doesn't already have —
  the whole point of a JWT + JSON REST API is that it needs no cookie/interceptor
  machinery axios is best known for). Redux/Zustand/Recoil (real overhead for a single
  auth object). Tailwind/MUI/Chakra (a styling *system* is a lot to learn just to build
  four pages; revisit if Phase 14's dashboards make plain CSS genuinely unwieldy).
  `AllowCredentials()` on the CORS policy (unnecessary and stricter-than-needed once you
  add it — the token travels in `Authorization`, never a cookie, so there's nothing for
  credentialed CORS to protect here).
- **Also decided alongside this:** the stored JWT's `expiresAt` (already returned by
  `AuthResponseDto` since Phase 6) is checked client-side on every app load
  (`AuthContext`'s `readStoredAuth`) — an expired stored token is treated as logged out
  immediately, rather than showing a logged-in UI that then fails on the first API call.
  This is a client-side courtesy only; it doesn't change the backend's stateless-JWT
  limitation documented in D16/`LEARNING_LOG.md` Phase 11 (a *disabled* user's
  not-yet-expired token still works against the API until it naturally expires — the
  frontend has no way to know that without asking the API).

## D19 — Protected routes via one wrapper component; a new backend endpoint to support the company's edit form
- **Decision:** Route protection (Phase 14's dashboards) is one small component,
  `ProtectedRoute`, used as a wrapper around each dashboard/company-management route:
  not logged in → redirect to `/login`, stashing the attempted location in router state
  so `LoginPage` can send the user back afterward; logged in as the wrong role →
  redirect to `/` (the public listing), not an error page. Separately, the backend
  gained one new endpoint, `GET /api/companies/me/internships/{id}`
  (`IInternshipService.GetOwnedByIdAsync`), because the company dashboard's "edit
  internship" form needs a post's full details regardless of status, and the existing
  public `GET /api/internships/{id}` only ever returns `Open` posts (Phase 8) — it would
  404 on a company's own Draft or Closed post.
- **Why:** A single wrapper component is the standard React Router pattern for this and
  needs no new state beyond what `AuthContext` (D18) already exposes
  (`isAuthenticated`, `role`) — there is nothing here that justifies a routing library
  add-on or a more elaborate permissions system at this project's scale (three roles,
  a handful of routes). Redirecting a role-mismatch to `/` rather than a "403" page
  reflects that visiting the wrong dashboard isn't an error condition the way a failed
  API call is — it's just the wrong page for that user, and the public listing is
  always a safe, valid place to land. The new backend endpoint exists because
  "let a company edit its own Draft post" is a real, necessary capability the API
  didn't yet expose — discovered while building the edit form, not part of the original
  Phase 14 plan, but a small, contained, ownership-checked addition (mirrors the
  existing `Update`/`Delete`/`Open`/`Close` ownership pattern exactly) rather than a
  workaround (like relaxing the public endpoint's `Open`-only rule, which would break
  the actual reason that rule exists — see D-notes on Phase 8).
- **Rejected:** A more general-purpose route-permissions table/config system (e.g.
  mapping every path to allowed roles centrally) — meaningful at dozens of routes, not
  at the eight this phase adds. Relaxing `GET /api/internships/{id}` to allow the owner
  through regardless of status (would tangle an intentionally simple public endpoint
  with an owner-specific special case; a separate endpoint under `/companies/me/...`
  keeps the two concerns — "what the public can see" vs. "what a company can see of its
  own" — as separate as they already are for the *listing* endpoints).
- **Also decided alongside this:** login/registration now redirect to `/dashboard` (a
  small role-dispatch page, `DashboardRedirectPage`) instead of the public listing from
  Phase 13 — now that dashboards exist, that's the more useful landing spot; the public
  listing is still one click away via the navbar's "Browse Internships" link, added this
  phase alongside "Dashboard".

## D20 — Service-layer unit tests against EF Core's InMemory provider, not a real Postgres database
- **Decision:** `backend/tests/InternshipManagement.Tests` (xUnit) tests every service
  directly - `new AuthService(db, fakeJwt)`, `new InternshipService(db)`, etc. - against
  a fresh `Microsoft.EntityFrameworkCore.InMemory` database per test
  (`TestDbContextFactory.Create()`, a new Guid-named database every call, so tests never
  share state). No mocking framework: `AppDbContext` is used for real (just backed by an
  in-memory store instead of Postgres), and `IJwtTokenGenerator` gets one small hand-written
  fake (`FakeJwtTokenGenerator`) since token *signing* isn't a business rule any service
  under test owns. Two things are deliberately **not** covered by these tests, both
  because the InMemory provider can't reproduce them: `EF.Functions.ILike`-based query
  filters (`InternshipService.GetAllAsync`'s location/search filters - Postgres-specific
  syntax the InMemory provider can't translate) and the duplicate-application
  race-condition fallback (`ApplicationService.ApplyAsync`'s `catch (... PostgresException
  ...)` block, which needs a real Postgres unique-constraint violation to trigger). Both
  were already verified live against the real database in Phases 9 and 12 respectively.
- **Why:** The project's services take `AppDbContext` directly with no repository
  interface in between (a deliberate simplicity choice from early phases), so "unit
  testing a service" necessarily means giving it a real `DbContext` — the question is
  only which database backs it. EF Core's InMemory provider still enforces the unique
  indexes configured in `AppDbContext.OnModelCreating` (composite/unique constraints
  aren't just a Postgres-only detail), runs in milliseconds, needs no connection string
  or running database, and is exactly what lets `dotnet test` work for anyone who clones
  this repo without having set up Postgres yet — a meaningful bar for a portfolio
  project. A real Postgres test database would exercise 100% of the code paths
  (including the two gaps above) but turns every test run into an integration test
  dependent on external infrastructure being up and correctly migrated - a heavier
  setup this phase's scope doesn't call for, given the two gaps are narrow, already
  covered by manual testing, and explicitly documented rather than silently untested.
- **Rejected:** A mocking framework (Moq/NSubstitute) for `AppDbContext` - mocking an
  `IQueryable`-based API convincingly is notoriously awkward and would end up testing
  "did the code call the mock the way I told the mock to expect" rather than real query
  behavior; a real (in-memory) database is both simpler to set up and more honest here.
  A real Postgres test database/Testcontainers setup - more faithful, but meaningfully
  heavier infrastructure than a ⭐⭐⭐-difficulty phase calls for; worth reconsidering only
  if a future phase specifically needs to test Postgres-specific behavior end-to-end.
  Repository interfaces purely to make mocking easier - would mean introducing a layer
  of abstraction into the *production* code that exists only to serve tests, reversing
  an intentional simplicity decision from early phases for testing's sake alone.
- **Also decided alongside this:** shared entity-seeding helpers
  (`TestHelpers/EntityFactory.cs` - `CreateStudentAsync`, `CreateCompanyAsync`,
  `NewPost`) live in one file used by every test class, rather than each test file
  hand-rolling its own near-identical "make a User + profile" boilerplate - the same
  reasoning as any other DRY cleanup, just applied to test code instead of production
  code. No `.sln` file was added - `dotnet test backend/tests/InternshipManagement.Tests`
  and `dotnet build`/`dotnet run --project ...` for the API follow the same
  explicit-project-path convention this repo has used since Phase 1, rather than
  introducing solution-file tooling for two projects where it isn't needed.

## D21 — Docker Compose topology: nginx reverse-proxies the frontend to the backend; the backend always migrates, seeds, and serves Swagger
- **Decision:** `docker compose up` runs three services - `postgres` (18-alpine, its own
  named volume, its own host port 5433 so it never collides with a developer's native
  Postgres on 5432), `backend` (multi-stage Dockerfile, `ASPNETCORE_ENVIRONMENT=Production`,
  connection string and JWT key from environment variables via `.env` - never committed,
  same posture as User Secrets in D11/D12), and `frontend` (multi-stage Dockerfile: Vite
  build, then nginx serving the static output). Crucially, **nginx also reverse-proxies
  `/api/*` to the backend container** - the browser only ever talks to nginx's one
  origin, for both the site and its API calls, so the Dockerized stack needs no CORS at
  all (unlike the `npm run dev` + `dotnet run` workflow from Phase 13, which still does
  and keeps its own `AllowOrigins("http://localhost:5173")` policy unchanged - the two
  setups coexist, each with the CORS posture it actually needs). On the backend, three
  behaviors that used to be `Development`-only now run in **every** environment: EF
  Core migrations apply automatically at startup (`Database.MigrateAsync()`), the admin
  account seeds (already idempotent), and Swagger stays enabled.
- **Why:** A reverse proxy in front of both the static site and the API is what a real
  deployment topology actually looks like (one public origin, an API behind it) -
  closer to "production simulation" than teaching the production container to accept
  cross-origin browser requests just to dodge the question. Auto-migration exists
  because a fresh Postgres container starts with an empty schema and there's no
  developer sitting at a terminal inside it to run `dotnet ef database update` by hand;
  making that unconditional (not just "when Dockerized") means local development gets
  the same simplification instead of a second, subtly different code path to keep in
  sync. Swagger staying on everywhere is a deliberate, explicit call for *this* project
  specifically: `docs/PROJECT_SCOPE.md`'s success definition is being able to demo and
  explain the system, and Swagger is the primary tool this project has used for that
  since Phase 1 - locking it to `Development` would make the very stack meant to
  demonstrate the finished system unable to demonstrate its own API.
- **Rejected:** Running the Docker backend with `ASPNETCORE_ENVIRONMENT=Development`
  instead of restructuring `Program.cs` - would have made Swagger/seeding "just work"
  with zero code changes, but mislabels a "production simulation" as literally
  Development, and every future environment-conditional decision would inherit that
  same confusion. Giving the Dockerized frontend its own CORS-permissive setup instead
  of a reverse proxy - simpler to wire up, but teaches the less realistic pattern for a
  phase explicitly about simulating production. A shared host port between the
  Dockerized Postgres and the native one - would force a developer to stop their own
  Postgres before ever running `docker compose up`, which defeats "runs alongside your
  normal dev setup."
- **Also discovered this phase:** the official `postgres:18-alpine` image changed its
  expected volume mount point from `/var/lib/postgresql/data` to `/var/lib/postgresql`
  (it now manages a version-specific subdirectory itself, to support in-place major
  version upgrades via `pg_upgrade --link`) - mounting at the old, pre-18 path makes the
  entrypoint refuse to start at all, assuming a botched upgrade. Found by reading the
  container's own error message on first run and fixed by mounting one level higher.

## D22 — Deployment (Render): runtime env-config for the frontend, configurable CORS, and a Postgres URI normalizer
- **Decision:** Three changes made Phase 16's images deployable to Render as-is,
  without a Render-specific image. (1) The frontend no longer bakes its API URL into
  the JS bundle at Docker *build* time (Render's Blueprint spec has no way to pass a
  Docker `--build-arg`, confirmed by reading Render's own `render.yaml` JSON schema -
  it exposes `dockerfilePath`/`dockerContext`/`dockerCommand` but nothing for build
  args). Instead, `env-config.template.js` gets turned into the real `env-config.js`
  the browser loads by a script under `/docker-entrypoint.d/` (the official nginx
  image's own hook mechanism, using `envsubst`, already present in that image for its
  own config-templating feature) - at container *startup*, from an `API_BASE_URL`
  environment variable. `client.ts` reads `window.__ENV__.API_BASE_URL` first, falling
  back to the Vite build-time variable only for `npm run dev` (which has no container
  or entrypoint to populate `window.__ENV__` at all). (2) The backend's CORS origins
  moved from a hardcoded `"http://localhost:5173"` to a configurable
  `Cors:AllowedOrigins` (comma-separated), so the deployed frontend's real
  `*.onrender.com` origin can be added via one Render environment variable, no code
  change or rebuild required. (3) `ConnectionStringHelper.ToNpgsqlConnectionString`
  normalizes Render's Postgres connection string, handed out as a URI
  (`postgres://user:pass@host:port/db`), into the `Host=...;Port=...;...` keyword
  format Npgsql's `UseNpgsql` actually expects - anything not starting with
  `postgres(ql)://` (local dev, Docker Compose) passes through completely unchanged.
- **Why:** (1) is a direct consequence of a real platform limitation, not a design
  preference - once Docker build args weren't available, *some* form of runtime
  configuration was the only way the same image could still work in more than one
  environment (Docker Compose locally, Render publicly) without maintaining two
  separate frontend images. (2) exists because the deployed frontend's origin is a real
  domain that doesn't exist until Render creates the service - hardcoding a guess into
  source code and redeploying if wrong is worse than one environment variable set once
  in the dashboard. (3) exists because Npgsql and "a cloud provider's connection
  string" are simply two different, incompatible string formats for the same
  information - something has to bridge them, and doing it in one small, directly
  tested helper (see `ConnectionStringHelperTests`) is simpler than trying to configure
  Npgsql to accept a format it doesn't parse.
- **Rejected:** Keeping the Phase 16 build-arg approach and building a *second*,
  Render-specific frontend image - works, but means two Dockerfiles (or one with
  environment-specific branches) to keep in sync for what's fundamentally the same
  static site. Hardcoding the deployed frontend's origin directly into `Program.cs` -
  couples application source code to a specific deployment's domain name, breaking the
  moment that domain changes for any reason. A Postgres connection pooler or ORM-level
  URI support instead of `ConnectionStringHelper` - Npgsql's own `UseNpgsql` simply
  doesn't accept the URI form directly, and reaching for a bigger dependency to solve a
  fifteen-line parsing problem isn't warranted here.
- **Also decided alongside this:** Render's free tier was chosen deliberately for an
  initial public demo, with its real limitations (a free Postgres instance expires 30
  days after creation, and free web services spin down after inactivity, cold-starting
  slowly on the next request) documented plainly in `docs/DEPLOYMENT.md` rather than
  hidden - upgrading either resource later needs no code change, only a plan change in
  Render's dashboard.

---