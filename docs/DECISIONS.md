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

---