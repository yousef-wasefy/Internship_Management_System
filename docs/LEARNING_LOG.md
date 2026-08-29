# Learning Log

A running record of what was learned in each phase. The point of this project is
**understanding**, so each phase adds a short entry: what was new, what was confusing, and
one thing you could now explain in an interview.

Template for each entry:

```
## Phase N — <name> (<date>)
- **New concepts:** ...
- **What confused me / how I resolved it:** ...
- **Could now explain in an interview:** ...
```

---

## Phase 0 — Project Scope & Requirements (2026-07-05)
- **New concepts:** Why scope and requirements are written *before* code; the difference
  between MVP (must-have) and out-of-scope (deliberately delayed) features; role-based
  requirements; capturing decisions as ADRs.
- **What confused me / how I resolved it:** _(fill in)_
- **Could now explain in an interview:** What the project is, who uses it, what ships in v1,
  and what was intentionally left out — and why each locked-in tech decision was made
  (see `DECISIONS.md`).

## Phase 1 — Environment & Backend Skeleton (2026-07-11)
- **New concepts:** What a Web API project is (an HTTP server that returns data, not HTML
  pages); what Swagger/OpenAPI is (a machine-readable description of the API's endpoints,
  rendered as an interactive test-it-yourself web page); the Controllers/Program.cs
  skeleton `dotnet new webapi` generates; solutions (`.slnx`) vs. projects (`.csproj`) —
  a solution groups one or more projects (we'll add a test project here in Phase 15).
- **What confused me / how I resolved it:** The default .NET 10 template does **not**
  include a Swagger UI — it only wires up `Microsoft.AspNetCore.OpenApi`, which serves the
  raw JSON spec with no page to click through. Swapped it for `Swashbuckle.AspNetCore`
  (`AddSwaggerGen()` + `UseSwagger()` + `UseSwaggerUI()`), which is the classic interactive
  UI at `/swagger`. Also `dotnet new sln` now creates a `.slnx` file (a newer, simpler XML
  solution format) instead of the classic `.sln` — same purpose, newer format.
- **Could now explain in an interview:** The difference between a Web API and a website;
  why Swagger is useful during development (test endpoints without writing a frontend);
  what `dotnet build` vs `dotnet run` do; why the app currently reports "No operations
  defined in spec" (no controllers exist yet — that's Phase 5).

## Phase 2 — Just-Enough C# (2026-07-15)
- **New concepts:** Auto-properties (`public string X { get; set; }`) as C#'s built-in
  get/set — no hand-written accessor methods like Java. Object initializer syntax
  (`new Internship(...) { Status = InternshipStatus.Open }`) for setting extra properties
  right after construction. `List<T>` as the generic collection (same idea as Java's
  `List<T>` / C++'s `std::vector<T>`). `enum` as a closed, named set of values instead of
  raw strings/ints — the compiler rejects anything not in the set. An `interface`
  (`IApplicationValidator`) as a contract with no implementation; a class `: IInterface`
  promises to fulfill it. `async`/`await` and `Task<T>` for non-blocking I/O-bound work.
  Top-level statements (`Program.cs` with no `Main` method or class wrapper) as C#'s
  modern, minimal entry point.
- **What confused me / how I resolved it:** In C++, `class` members are private by
  default and you write explicit getters/setters; in C#, auto-properties give you that
  for free with one line, and members are `private` by default too but properties are
  usually `public` on purpose (that's the point — controlled *public* access to backing
  data). Also: `async` methods don't run on a separate thread by magic — `await
  Task.Delay(200)` frees the calling thread to do other work while waiting, then resumes
  where it left off; it's not the same as spawning a new thread.
- **Could now explain in an interview:** What a class/property/constructor is; the
  difference between a `class` and a `List<T>` of that class; why `enum` beats magic
  strings for status fields; why services depend on an `interface`
  (`IApplicationValidator`) instead of a concrete class — so the real
  `ApplicationService` (Phase 9) can be unit-tested against a fake validator without a
  real database; why `async`/`await` matters specifically for database calls (EF Core
  queries from Phase 4 onward are all `async Task<T>`); and the difference between an
  **entity** (a class that maps to a database table, coming in Phase 4) and a **DTO**
  (a class shaped for what an API request/response should look like, coming in Phase 5)
  — the practice classes here (`Student`, `Company`, ...) are neither yet; they're just
  syntax practice.

## Phase 3 — Database Design (2026-07-16)
- **New concepts:** Reading and writing an ERD (entity-relationship diagram) — boxes are
  tables, lines are relationships, and the little symbols on each end of a line (`||`,
  `o{`, ...) encode cardinality ("exactly one", "zero or many"). The difference between a
  **primary key** (uniquely identifies a row in its own table) and a **foreign key**
  (a column that points at another table's primary key). A **composite unique
  constraint** — a uniqueness rule across *two columns together*, not each column alone
  (`(StudentId, InternshipPostId)` can repeat StudentId many times and InternshipPostId
  many times, just never the same *pair* twice). What a **join table** is and why
  many-to-many relationships always need one in a relational database.
- **What confused me / how I resolved it:** At first it seemed like the duplicate-application
  rule ("a student can't apply twice") was purely an *application* rule to check in code.
  Realized the database can enforce it too, independently, via the composite unique
  constraint — so even a bug in the service code couldn't let a duplicate through; the
  database would reject the insert. This is "defense in depth" — checking in two places
  for the same rule, so one broken layer doesn't silently break the rule.
- **Could now explain in an interview:** Why `InternshipApplication` is a join table (and
  not just two foreign keys sitting somewhere) — it carries its own data (`Status`,
  `CoverLetter`, timestamps) rather than only recording that a link exists. Why `User` is
  a separate table from `StudentProfile`/`CompanyProfile` instead of one giant table
  (avoids a sparse table full of nulls, keeps auth data isolated from profile data). Why
  the schema uses `int` primary keys instead of `Guid` (simplicity — easier to read/type
  while testing). Why `Skills` is a single string field instead of a normalized table for
  v1 (avoiding over-engineering a feature nothing in the MVP actually needs yet).

## Phase 4 — EF Core Setup & First Migration (2026-07-18)
- **New concepts:** `DbContext` as the bridge between C# objects and database tables —
  one `DbSet<T>` property per table. EF Core's *convention-based* relationship discovery:
  because `StudentProfile.User` and `User.StudentProfile` are both single references (not
  lists), EF Core automatically treats it as a 1-to-1 relationship without being told —
  only the *uniqueness* of the `UserId` index had to be configured explicitly in
  `OnModelCreating`. What a **migration** actually is: a C# file with an `Up()` method
  (what to run to apply the change) and a `Down()` method (how to undo it), plus a
  `ModelSnapshot` file that tracks the model's current shape so the next migration only
  contains the *difference*. What **.NET User Secrets** is and why it exists (a way to
  keep real local credentials completely outside the git repository, not just
  `.gitignore`d inside it).
- **What confused me / how I resolved it:** Running `dotnet ef database update` printed a
  scary-looking `fail:` line before succeeding — turned out to be EF Core's own
  first-run check (querying its internal `__EFMigrationsHistory` table, which doesn't
  exist yet on a brand-new database), not a real error; it's expected and harmless on the
  very first migration. Also: the FK delete behavior defaulted to `CASCADE` for every
  relationship without being asked — that's EF Core's convention for *required*
  relationships (a `StudentProfile` can't exist without its `User`, so deleting the `User`
  cascades). This is a sensible default for now since nothing in this project hard-deletes
  rows yet (disabling/rejecting uses boolean flags instead).
- **Could now explain in an interview:** The difference between *designing* a schema
  (Phase 3, no tools involved) and *migrating* it (Phase 4, `dotnet ef migrations add`
  generates code, `dotnet ef database update` runs it against a real database). Why a
  password should never be typed directly into a file that's already tracked by git, and
  what tool (User Secrets) solves that for local development specifically. Why creating a
  dedicated, least-privilege database role for the app (instead of using the Postgres
  superuser everywhere) is safer — the app can only ever do what that one role is allowed
  to do. Proved — not just assumed — that the composite unique constraint actually
  rejects a duplicate application by inserting one directly with `psql` and watching
  PostgreSQL reject the second insert with `duplicate key value violates unique
  constraint`.

## Phase 5 — Internship CRUD API (2026-07-22)
- **New concepts:** The controller → service → `DbContext` split in practice for the
  first time — `InternshipsController` only translates HTTP requests into service calls
  and results back into HTTP responses; every actual decision (which company owns a new
  post, how to map an entity to a DTO) lives in `InternshipService`. Why DTOs exist as
  *separate* classes from entities: `CreateInternshipDto` deliberately has no `Status` or
  `CompanyId` field, because a client should never be able to set those directly — the
  server decides them. `[ApiController]` + `ActionResult<T>` conventions:
  `Ok()`/`NotFound()`/`CreatedAtAction()`/`NoContent()` map directly to the HTTP status
  codes (200/404/201/204) a REST API is expected to return. EF Core "relationship
  fixup": setting `post.Company = company` (a navigation property) before saving
  automatically fills in `post.CompanyId` — no need to set the foreign key by hand.
- **What confused me / how I resolved it:** Sending an `applicationDeadline` without a
  timezone offset (e.g. `"2026-12-31T00:00:00"` instead of `"...Z"`) crashed the save
  with an Npgsql error, because the column is `timestamp with time zone` and Npgsql
  refuses a `DateTime` whose `Kind` isn't explicitly `Utc`. Fixed with a small
  `AsUtc(...)` helper that treats an unspecified timezone as UTC rather than throwing.
  Also: after creating and then rolling back test rows in Phase 4, the *next* real row
  didn't get `Id = 1` as expected — it got `Id = 2`. This isn't a bug: PostgreSQL's
  identity/sequence counters are **not transactional** — a `ROLLBACK` undoes the row
  data but not the sequence's internal counter, so "the next id" can jump ahead of what
  you'd naively expect after any rolled-back insert.
- **Could now explain in an interview:** Why controllers should stay "thin" and business
  logic belongs in services — testability (Phase 15 will unit-test `InternshipService`
  without needing a running web server) and reuse (multiple controllers could call the
  same service). Why a temporary, clearly-commented seed (`SeedData.cs`) is a reasonable
  way to unblock CRUD testing before auth exists, as long as it's deliberately temporary
  and documented as such. Why enums were configured to serialize as strings
  (`"Remote"`) instead of raw numbers (`1`) in JSON — dramatically easier to read and
  test against, at the cost of one line of startup configuration.

## Phase 6 — Authentication & Roles (2026-07-23)
- **New concepts:** **Authentication** (proving who you are - login) vs.
  **authorization** (what you're allowed to do once identified - roles). What a JWT
  actually is: three base64 sections (header.payload.signature) - the payload is a set
  of **claims** (email, role, ...) about who the token represents, and the signature
  (computed with a secret key only the server knows) is what makes it tamper-evident,
  not encrypted or secret in itself - anyone can *read* a JWT's claims, they just can't
  *forge* a valid signature without the key. Why passwords are hashed with BCrypt (a
  one-way function - the server never stores or can recover the original password, only
  verify a guess against the hash) instead of encrypted (reversible, and therefore a
  liability if the encryption key ever leaks). `[Authorize]` vs.
  `[Authorize(Roles = "Company")]` - the former means "any valid token," the latter
  means "a valid token whose role claim matches."
- **What confused me / how I resolved it:** Adding Swagger's "Authorize" button broke
  the build entirely - the tutorial-standard `Microsoft.OpenApi.Models` namespace and
  `OpenApiSecurityScheme.Reference` property no longer exist, because Swashbuckle 10 /
  Microsoft.OpenApi 2.x (pulled in by earlier phases) reworked how references work.
  Fixed by researching the actual current API: the namespace is now just
  `Microsoft.OpenApi` (no `.Models`), and building a security requirement now needs a
  `document => new OpenApiSecurityRequirement { [new
  OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>() }` callback
  instead of a plain object with a `Reference` property. A good reminder that a library
  major-version bump can silently invalidate code that "should" work from older
  tutorials/examples - always check what version is actually installed. Also: ASP.NET
  Core silently *remaps* JWT claim names on the way in unless you turn it off
  (`MapInboundClaims = false`) - without that, code reading back the exact claim name
  used to create the token would mysteriously get `null`.
- **Could now explain in an interview:** The full request lifecycle for a protected
  endpoint - client sends `Authorization: Bearer <token>` → `UseAuthentication()`
  validates the signature/expiry and builds a `ClaimsPrincipal` → `[Authorize(Roles=...)]`
  checks the role claim → the action runs, or the request is rejected with 401
  (not authenticated) or 403 (authenticated, wrong role) before the controller code ever
  executes. Why login returns the identical error message for "wrong password" and
  "no such account" (never reveal which one - avoids leaking which emails are
  registered). Why `AuthService` methods return `null` for business-rule failures
  instead of throwing - consistent with the pattern already used in `InternshipService`
  (Phase 5), letting the controller translate `null` into the right HTTP status.

## Phase 7 — Student & Company Profiles (2026-07-24)
- **New concepts:** Looking a row up by a *foreign* key instead of its own primary key -
  `StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId)`, not `FindAsync(id)`,
  because the JWT only ever encodes the logged-in *user's* id, never a profile's own id.
  Controller-level `[Authorize(Roles = "...")]` (applied once, above the whole class) vs.
  action-level (applied per-method, as in `InternshipsController`) - the right choice
  depends on whether *every* action in a controller needs the same restriction
  (`StudentsController`/`CompaniesController`: yes, every route is a "me" route only that
  role could ever meaningfully call) or only *some* do (`InternshipsController`: `GET` is
  public, only the writes need a role). Reusing one service (`ICompanyService`) from two
  different controllers (`CompaniesController` for self-service, `AdminController` for
  admin actions) instead of writing a separate, near-duplicate `AdminService` for a
  single method.
- **What confused me / how I resolved it:** Nothing new broke this phase - it followed
  the same controller → service → `DbContext` shape as Phase 5's `InternshipService`
  almost exactly, which made this the most mechanical phase so far. That *is* the
  lesson: once the pattern is understood once, repeating it for a new resource
  (profiles) is mostly copy-adapt-verify, not new problem-solving.
- **Could now explain in an interview:** Why `UpdateCompanyProfileDto` has no
  `IsApproved` field at all (not just "ignored if sent") - the same
  DTO-as-a-boundary reasoning from Phase 5's `CreateInternshipDto` having no `Status`
  field. Why the admin approve endpoint takes the `CompanyProfile`'s own id in the route
  (`/api/admin/companies/{id}/approve`) rather than a `UserId` - an admin is acting on
  "a company" from a future list (Phase 11), not resolving "themselves" the way `/me`
  endpoints do. Why starting a minimal `AdminController` now (one action) instead of
  waiting for Phase 11 to create it from scratch avoids the awkward alternative of
  putting a temporary admin action somewhere unrelated (like `CompaniesController`)
  just because the "real" controller doesn't exist yet.

## Phase 8 — Internship Publishing Workflow (2026-07-27)
- **New concepts:** Modeling a **status workflow** (`Draft → Open → Closed`) as
  application-layer rules on top of a plain enum column, rather than a database
  state-machine feature - PostgreSQL doesn't know or enforce that `Closed → Open` is
  fine but `Cancelled → Open` isn't; that logic lives entirely in
  `InternshipService.OpenAsync`. The difference between a **404** (resource doesn't
  exist - or, for the public endpoints this phase, "exists but you're not allowed to
  know that" is deliberately folded into the same 404) and a **403** (resource exists,
  you're authenticated, but you specifically aren't allowed to act on *this* one) - and
  why ownership checks are a 403 case, not a 404 case, when the resource isn't secret
  (an internship's existence is already public once `Open`). C# **named tuple returns**
  (`Task<(OperationResult Result, string? ErrorMessage, InternshipDetailsDto? Internship)>`)
  as a lightweight way to return multiple related values from a method without defining
  a whole new class for it.
- **What confused me / how I resolved it:** Realized partway through that making the
  public listing show only `Open` posts silently broke something: a company would have
  *no way at all* to see its own Draft posts anymore (the only endpoint that could show
  them, `GET /api/internships`, now filters them out for everyone, owner included).
  This wasn't a bug to fix so much as a missing endpoint to add - `GET
  /api/companies/me/internships`, which turned out to already be named in the original
  project brief's endpoint list even though no phase had scheduled it yet. A reminder
  that removing visibility from one endpoint sometimes means a companion endpoint is
  needed, not just a filter.
- **Could now explain in an interview:** Why `OperationResult` has four cases instead of
  reusing the Phase 5/6 nullable-return pattern - once a method can fail for
  *structurally different reasons* that need *different HTTP status codes* (missing vs.
  not-yours vs. against-the-rules), a single `null`/`bool` can't carry enough
  information for the controller to respond correctly. Why the four publishing-rule
  checks in `OpenAsync` are ordered the way they are (existence → ownership → cancelled
  → approval → content → deadline) - each check assumes everything before it already
  passed, so ordering them from "cheapest and most fundamental" to "most specific"
  avoids doing expensive or confusing work before a simpler check would have already
  rejected the request. Why `Cancelled` currently has no way to be reached through the
  API at all, and why that's a known, documented gap rather than something Phase 8 was
  responsible for fixing (no endpoint in the entire project plan sets it).

## Phase 9 — Student Application Workflow (2026-08-07)
- **New concepts:** **Defense in depth** in practice, not just in theory - the duplicate-
  application rule is checked *twice*: a friendly `AnyAsync` pre-check (for the normal
  case, giving a clean 400 with a clear message) *and* a `catch` around `SaveChangesAsync`
  for the specific Postgres unique-violation error code (`23505`, via
  `PostgresErrorCodes.UniqueViolation`), which is what actually stops a duplicate if two
  requests from the same student race each other and both pass the pre-check before
  either commits. A rule that depends on **wall-clock time, not just state** - "the
  deadline has passed" isn't reflected anywhere in the `Status` column (nothing
  auto-closes a post), so it has to be checked as its own separate condition
  (`ApplicationDeadline <= DateTime.UtcNow`) every time, not inferred from `Status`.
  Why `POST /api/internships/{id}/apply` deliberately returns `201` with **no `Location`
  header** - there's no single-resource `GET` endpoint for an application yet to point
  the header at, and fabricating one that doesn't work would be worse than omitting it.
- **What confused me / how I resolved it:** Needed to test "student cannot apply after
  the deadline," but the API itself has no way to *create* that situation - `OpenAsync`
  (Phase 8) refuses to open a post whose deadline is already in the past, so you can
  never reach "an Open post past its deadline" purely through normal API calls in a
  single test run. Resolved by directly updating the row's `ApplicationDeadline` via
  `psql` to simulate time passing, then calling `apply` through the real API - a
  reasonable, common technique for testing time-dependent rules without literally
  waiting for a deadline to elapse.
- **Could now explain in an interview:** Why "not open" (Draft/Closed/Cancelled) and
  "deadline passed" are two *separate* checks even though a caller might describe both
  as "I can't apply" - they're independent facts about the internship that can each be
  true without the other. Why the duplicate check happens *after* the deadline check in
  `ApplyAsync`, not before - each check assumes the ones before it already passed
  (same "cheapest/most fundamental first" ordering principle from Phase 8), so a student
  re-applying to an internship whose deadline just passed gets the deadline message, not
  the duplicate message - the more fundamentally-blocking reason wins. Why `Forbidden`
  only applies to *withdraw* (a student acting on someone else's application) and not to
  *apply* (there's no ownership concept to violate when creating your own new
  application) - `OperationResult.Forbidden` is only meaningful where an existing
  resource has an owner to check against.

## Phase 10 — Company Application Review (2026-08-14)
- **New concepts:** The same underlying data (an `InternshipApplication`) needs a
  genuinely **different DTO shape depending on who's looking at it** -
  `ApplicationDto` (Phase 9, the student's own view: which internship, what status) vs.
  `ApplicantDto` (this phase, the company's view: *who applied* - name, email,
  university, skills - since a company reviewing candidates cares who the person is, not
  just that "an application" exists). **Partial-update semantics on a single field**:
  `UpdateApplicationStatusDto.CompanyNotes` only overwrites the stored note when a new
  one is actually provided (`if (dto.CompanyNotes is not null)`), so shortlisting with a
  note and later accepting without repeating it doesn't silently erase the earlier note
  - a small but real departure from every previous `Update*` method in the project,
  which always overwrote every field unconditionally.
- **What confused me / how I resolved it:** Nothing broke this phase - by this point the
  ownership-check-then-business-rule shape (`NotFound` → `Forbidden` →
  `ValidationFailed` → `Success`) from Phase 8/9 applied directly to
  `UpdateStatusAsync`, just checked through a different path
  (`application.InternshipPost.Company.UserId`, not `application.Student.UserId`). The
  one genuinely new decision was ordering: check "is this application withdrawn"
  *before* checking "is the requested status value even legal" - because a withdrawn
  application should be rejected the same way no matter what status was requested,
  rather than sometimes returning a "withdrawn" message and sometimes an "invalid
  status" message depending on what the caller happened to send.
- **Could now explain in an interview:** Why `ApplicationsController` has **no**
  controller-level `[Authorize]`, unlike `StudentsController`/`CompaniesController` -
  because this is the first controller whose actions genuinely need *different* roles
  (`GetMy`/`Withdraw` need Student, `UpdateStatus` needs Company), so the restriction has
  to live on each action individually. Why `GET /api/companies/me/applications` and
  `GET /api/internships/{id}/applications` are two different endpoints instead of one -
  the first answers "show me everything, across all my posts" (a dashboard-style view,
  no id needed since it's implicitly "mine"), the second answers "show me applicants for
  *this specific* post" (needs an id, and therefore needs an ownership check the first
  one doesn't). Why `OperationResult.Forbidden` for `GetApplicantsForInternshipAsync`
  checks the *internship's* owner, while `UpdateStatusAsync`'s check reaches the same
  owner through the *application's* `InternshipPost.Company.UserId` - two different
  navigation paths arriving at the same ownership fact, because the two methods start
  from different resources (an internship id vs. an application id).

## Phase 11 — Admin Dashboard and Management (2026-08-16)
- **New concepts:** **Aggregation queries** (`CountAsync` with a filter) as the simplest
  possible "dashboard" - eight independent counts, each one line, rather than one
  complex combined query; readability winning over minimizing round-trips at this
  project's scale. Modeling a **missing schema state through an existing field** instead
  of adding a new one - there's no "Rejected" value anywhere in the database, so
  rejecting a company reuses the already-existing `IsDisabled` flag to achieve the same
  practical effect (the company can't use the platform), a deliberate reuse rather than
  a three-state field nothing else needs yet. The concrete difference between **stateless
  token validation** and **revocation** - a JWT is checked by verifying its signature and
  expiry, a purely local, no-database-lookup operation; that's exactly what makes JWTs
  fast and simple, and exactly why there's no way to "unvalidate" one early without
  adding back a database check somewhere (which is what `/auth/me` now does, and most
  other endpoints deliberately don't).
- **What confused me / how I resolved it:** Assumed disabling a user would immediately
  block *all* of their access, and was surprised when a disabled student's existing
  token still worked at `/auth/me`. This wasn't a bug in the disable logic - it revealed
  that `GetCurrentUserAsync` never checked `IsDisabled` at all (only `LoginAsync` did,
  since Phase 6). Fixed that one specific gap, but didn't chase it further into
  "make every endpoint re-check the database on every request," since that's a real
  architectural tradeoff (giving up JWT's normal statelessness) worth a deliberate future
  decision, not a quick patch bolted on to a "add a disable button" phase. Verified the
  fix by recreating the exact scenario: issue a token, disable the account, then reuse
  that specific pre-issued token - confirming `/auth/me` now correctly returns 401 while
  (deliberately, and now documented) every other endpoint still wouldn't.
- **Could now explain in an interview:** Why `ApproveCompanyAsync` moved out of
  `ICompanyService` into a new `IAdminService` this phase, and why that's a real
  refactor closing a documented Phase 7 stub, not scope creep. Why "reject" and "disable"
  are two different endpoints even though they change the same underlying field
  (`IsDisabled`) - they represent different *decisions* an admin can make (rejecting a
  company that never should have been approved, vs. disabling any user for any reason at
  any time), even though the schema doesn't distinguish them after the fact. Why a JWT
  can't be "revoked" the way a session cookie can without adding infrastructure
  specifically for that (a blocklist, or short-lived tokens with a refresh flow) - and
  why that's a reasonable, honestly-documented limitation for this project's scope
  rather than something to silently paper over.

## Phase 12 — Validation, Error Handling, and API Quality (2026-08-22)
- **New concepts:** **RFC 9457 Problem Details** as the standard shape for HTTP API
  errors - `type`/`title`/`status`/`detail`, recognized by any developer familiar with
  modern REST APIs, and already partially built into ASP.NET Core (automatic
  DataAnnotations validation has used this shape all along, unnoticed until this phase
  gave it something to validate). The difference between **DTO validation** ("is this
  request well-formed?" - a required field, a valid email format) and **business rule
  validation** ("is this action allowed right now?" - Phase 8/9/10's `ValidationFailed`
  cases) - both can produce a `400`, but they're checked at completely different layers
  ([ApiController]'s automatic model binding vs. hand-written service logic), which is
  *why* `CreateInternshipDto.ApplicationDeadline` has no "must be in the future"
  attribute even though that rule very much exists (just one layer deeper, in
  `OpenAsync`). The **global exception handler** (`IExceptionHandler`) as the
  *last-resort* safety net - every other error in the whole project is something a
  service deliberately decided to signal; this is the one and only place code written
  for a totally different, unanticipated failure gets caught before it would otherwise
  crash the request with a raw, unformatted error.
- **What confused me / how I resolved it:** Assumed unifying every controller's error
  responses to `Problem(...)` would make *every* 401/403 in the API consistent - then
  found, while testing, that a wrong-role `[Authorize(Roles = "Company")]` rejection
  still came back completely empty (no body at all), unlike the ownership-check 403s
  from inside controller actions. The reason: `[Authorize]` rejections are handled by
  ASP.NET Core's *authorization middleware*, entirely separate from - and running
  *before* - any controller code, so none of my `Problem(...)` calls were ever reachable
  for that specific path. Fixed by writing a custom `IAuthorizationMiddlewareResultHandler`
  that intercepts exactly that case and writes the same ProblemDetails shape. A concrete
  lesson that "add consistent error handling" isn't fully solved just by fixing every
  controller - some rejections never reach a controller at all.
- **Could now explain in an interview:** Why the duplicate-application check (Phase 9) and
  most other business rules were *not* moved into DataAnnotations even though both
  produce a `400` - DataAnnotations validate a single DTO in isolation, with no database
  access; "have you already applied" needs a database query, which is fundamentally a
  service-layer concern, not something an attribute on a property could ever check. Why
  `GET /api/internships`'s response shape (a bare array before Phase 12) had to become a
  wrapper object (`PagedResult<T>`) rather than adding pagination info as response
  *headers* - both are legitimate real-world patterns, but a body wrapper is simpler to
  explain and consume for a learning project, at the cost of being a breaking change to
  that one endpoint (clearly flagged in the docs). Why status filtering was added to the
  *company's own* internship listing but deliberately not to the public one, even though
  the original brief's Phase 12 wording suggested "add status filtering" without that
  distinction - the public listing's Open-only rule (Phase 8) is a security/privacy
  boundary, not a default that a query parameter should be able to override.

## Phase 13 — React Frontend Basics (2026-08-24)
- **New concepts:** **CORS** (Cross-Origin Resource Sharing) - the browser, not the
  server, is what blocks a fetch from `http://localhost:5173` to
  `http://localhost:5053` by default, because they're different origins (different
  ports count); the server has to explicitly opt in via response headers
  (`AddCors`/`UseCors`) naming which origins may call it. **React Context** as the
  minimum-ceremony way to share one piece of state (who's logged in) across components
  that aren't directly related in the tree (`Navbar`, `InternshipDetailPage`'s apply
  form) without prop-drilling it through every layer in between. The distinction
  between a **controlled component** (an `<input>` whose `value` comes from React state,
  and whose every keystroke calls `setState`) and letting the DOM own its own value -
  every form field in this phase is controlled, which is *why* a state variable exists
  for `email`, `password`, `search`, etc. even though the DOM technically stores the
  value too. **Client-side routing** (`react-router-dom`'s `<Routes>`) - the browser's
  URL bar changes without a full page reload; the server only ever serves one HTML
  file (`index.html`) and JavaScript decides what to render for each path.
- **What confused me / how I resolved it:** Registering a fresh account and then trying
  to "apply again" without logging out first didn't feel like it should be possible to
  test easily from the UI - but it turned out to be the perfect way to exercise the
  *duplicate application* error path end-to-end (`OperationResult.ValidationFailed` from
  Phase 9, surfaced through `Problem(...)` since Phase 12, now displayed by the
  frontend's `ErrorMessage` component) without needing to fabricate anything - just
  clicking "Submit application" on the same internship twice, as a real user might by
  accident. Separately, while live-testing the Register page's Student/Company toggle
  through browser automation, clicks that reported success weren't changing anything
  on screen; dispatching a real DOM click via JavaScript proved the component logic was
  correct all along - the mismatch was in how the automated testing tool was interacting
  with the page in that particular session, not a bug in the code (see
  `docs/phase-summaries/PHASE_13_SUMMARY.md` for how this was isolated).
- **Could now explain in an interview:** Why the frontend has its own `types/index.ts`
  hand-mirroring the backend's DTOs and enums instead of generating one from the OpenAPI
  spec Phase 12 already produces - a generated client is the "more correct" answer at a
  larger scale, but for seven endpoints, a generation step would be extra tooling to
  learn and debug for a project whose whole point is understanding each layer by hand.
  Why enums serialize as `"Open"` rather than `1` in every API response (Phase 12's
  `JsonStringEnumConverter`, in `Program.cs` since Phase 1) - and why that specific
  choice is *why* the frontend's `UserRole`/`WorkMode`/etc. types are plain string
  unions (`'Student' | 'Company' | 'Admin'`) instead of numeric enums. Why the JWT's
  `expiresAt` is checked client-side on every app load rather than trusting
  `localStorage` blindly - an expired-but-still-present token would otherwise render a
  "logged in" UI that fails on the very first real request, which is worse than just
  showing logged-out.

## Phase 14 — Role-Based Dashboards (2026-08-26)
- **New concepts:** **Route guards** as a plain component, not a framework feature -
  `ProtectedRoute` is just a component that returns either `<Navigate>` or its children
  based on `AuthContext`, proving that "protecting a route" in React Router is a
  rendering decision, not a special API. **Router state for a post-login redirect** -
  `<Navigate to="/login" state={{ from: location }} />` attaches data to a specific
  history entry (not global state), which `LoginPage` reads back via `useLocation()` to
  send the user to where they were actually headed, rather than always to one fixed
  page. **`localStorage` is shared per browser origin, not per tab** - logging into a
  second account in a second tab of the same browser silently logs the first tab into
  that same account too, the next time it does anything that reads storage (see below) -
  a real, common characteristic of any `localStorage`-based session (not unique to this
  app), which shaped how the rest of this phase's multi-role testing had to be done
  (one account logged in at a time, never two tabs with two different accounts open
  against the same origin at once).
- **What confused me / how I resolved it:** Logging in as a fresh admin account
  immediately after building the dashboard redirect landed on the public listing
  instead of the admin dashboard - looked exactly like the `DashboardRedirectPage`
  logic was broken. Inspecting `window.history.state` directly showed the real cause:
  that particular browser tab's navigation history still carried a *stale* redirect
  target (`from`) attached to an old `/login` history entry from much earlier in the
  same tab's session, and revisiting that same URL let the browser reuse the old entry's
  state instead of starting fresh. Testing the exact same login in a brand-new tab (no
  prior history at all) worked correctly on the first try, proving the redirect logic
  itself was right - the false alarm was an artifact of reusing one browser tab across
  many rounds of manual testing, not a bug in `LoginPage` or `DashboardRedirectPage`
  (see `docs/phase-summaries/PHASE_14_SUMMARY.md` for the full isolation steps). A
  second, unrelated mix-up during the same testing session: applicants for an internship
  briefly seemed to vanish after switching accounts - actually the `localStorage`
  sharing behavior described above, not a data problem; logging back in as the intended
  account made everything reappear exactly as left.
- **Could now explain in an interview:** Why `GET /api/internships/{id}` (public) and
  the new `GET /api/companies/me/internships/{id}` (Phase 14) both exist and return the
  same DTO shape, rather than making the public endpoint smarter about who's asking -
  keeping "what anyone can see" and "what an owner can see of their own, regardless of
  status" as two separate, simply-reasoned-about endpoints beats one endpoint with
  conditional visibility rules baked in. Why a role-mismatch on a protected route
  redirects to the public listing rather than showing a "403 Forbidden" page - the
  wrong dashboard for your role isn't a broken or unauthorized request the way a failed
  API call is, it's simply not the page for you, and the listing is always a safe,
  valid place to land instead. Why the company's applicant-review action always sends
  whatever is in the note field alongside the status change (rather than offering a
  separate "just save the note" action) - `UpdateApplicationStatusDto`'s `Status` field
  is not optional server-side, so there is no API call that could update *only* the
  note without also asserting a status; the UI's one combined action matches what the
  endpoint actually allows.

## Phase 15 — Testing (2026-08-27)
- **New concepts:** **xUnit's `[Fact]`/`Assert` model** as the standard shape of a .NET
  unit test - arrange (build the scenario), act (call the one thing under test), assert
  (check the result), one behavior per test. **EF Core's InMemory provider** as a way to
  give a service a real, working `DbContext` in milliseconds with no database server
  running - and, more subtly, that "in-memory" doesn't mean "no rules enforced": unique
  indexes configured in `OnModelCreating` are still checked, just not everything a real
  provider does (Postgres-specific query syntax like `EF.Functions.ILike` has nothing to
  translate to on an in-memory store). **Test isolation via a fresh database per test** -
  giving `UseInMemoryDatabase` a new random name every time means tests never see each
  other's data and can run in any order, in parallel, without any manual cleanup step
  (unlike this project's own manual live-testing routine every phase, which has always
  needed an explicit cleanup pass afterward).
- **What confused me / how I resolved it:** It wasn't obvious at first which parts of
  `ApplicationService.ApplyAsync`'s duplicate-application handling were even testable
  without a real Postgres database - the method has *two* layers of protection (an
  `AnyAsync` pre-check, and a `catch` block for the real database's unique-constraint
  violation as a race-condition fallback, from Phase 9). Working through it made the
  distinction clear: the pre-check is ordinary application logic, testable like any
  other branch; the `catch` block only exists to handle a database-level exception type
  (`PostgresException`) that only a real Postgres connection can ever throw, so it's
  fundamentally not something an InMemory-backed test can exercise - not a gap in the
  test suite's care so much as a boundary of what "unit test" versus "integration test"
  each cover, which is why it's called out explicitly in `docs/DECISIONS.md` D20 rather
  than silently left untested with no explanation.
- **Could now explain in an interview:** Why these are called *unit* tests and not
  *integration* tests, even though they use a real `AppDbContext` (a "unit" here is
  "one service's behavior in isolation," not "no dependencies at all" - the InMemory
  database stands in for a slow, external dependency the same way a mock would, just
  without needing to hand-write one for an `IQueryable`-based API). Why a mutation
  check is a meaningful way to trust a test suite - temporarily commenting out the
  approval-gate condition in `InternshipService.OpenAsync` and re-running the tests
  made `OpenAsync_UnapprovedCompany_ReturnsValidationFailed` fail immediately, proving
  the test was actually checking the rule rather than passing regardless of what the
  code does. Why the test project has no repository-interface layer inserted just to
  make mocking easier - that would mean changing the *production* code's architecture
  (removing direct `AppDbContext` access from services) purely to serve tests, when EF
  Core's InMemory provider already lets the real services be tested directly.

## Phase 16 — Docker & Local Prod Simulation (2026-08-29)
- **New concepts:** **Multi-stage Docker builds** - a build stage with the full SDK/Node
  toolchain produces an artifact (a published DLL, a `dist/` folder), then a second,
  much smaller stage (`aspnet` runtime, `nginx`) copies in only that artifact - the
  final image never contains a compiler, `node_modules`, or source code at all. **Build
  time vs. runtime configuration** for a frontend specifically - Vite's `VITE_*`
  variables get baked into the JavaScript bundle *when `npm run build` runs*, not read
  fresh every time the container starts, which is why the API base URL had to be a
  Docker build `ARG`, not a `docker-compose.yml` `environment:` entry (that would only
  affect the *nginx process*, which never reads it - the already-built JS never would
  either). **A reverse proxy as the reason two services can share an origin** - nginx
  serving the frontend's static files *and* forwarding `/api/*` to the backend
  container means the browser only ever sees one origin, which is what makes the whole
  CORS conversation from Phase 13 unnecessary for this specific setup (not solved -
  sidestepped, because the actual cross-origin request never happens here). **Docker
  Compose health checks and `depends_on: condition: service_healthy`** - the difference
  between "the Postgres container process has started" and "Postgres is actually ready
  to accept connections" is exactly the gap a plain `depends_on` (with no condition)
  doesn't cover, and the one this project's backend's own startup migration needed
  closed to avoid a race.
- **What confused me / how I resolved it:** The very first `docker compose up` attempt
  failed with Postgres refusing to start, citing "data in an unused mount/volume" -
  looked like a real problem with *this* setup specifically, but was actually the
  `postgres:18-alpine` image's own entrypoint script protecting against a known Docker
  Postgres pitfall (upgrading the image without upgrading the actual data via
  `pg_upgrade`), triggered here just because the compose file mounted the volume at the
  path Postgres images used *before* version 18, not the new one. Reading the
  container's own printed explanation (not just the exit code) pointed straight at the
  fix. A second moment: initially assumed the Dockerized frontend calling the API would
  need the exact same CORS setup as the Phase 13 dev workflow, until working through
  *why* CORS exists (different origins) made it clear that putting both services behind
  one nginx origin removes the precondition for CORS to even apply, rather than needing
  a Docker-specific CORS policy at all.
- **Could now explain in an interview:** Why the exact same `Program.cs` code
  (`Database.MigrateAsync()`, the admin seed, Swagger) now runs in every environment
  instead of being gated behind `IsDevelopment()` - and why that's a deliberate,
  documented choice for *this* project (a portfolio piece meant to be explored) rather
  than a security best practice being ignored. Why the Dockerized Postgres listens on a
  different host port (5433) than the native one used throughout development (5432) -
  and why that's about the *host* port only; inside the Docker network, the backend
  always reaches it as `postgres:5432` regardless. Why a `VITE_API_BASE_URL` of `/api`
  (a relative path) only makes sense once nginx is in front of both services - it would
  break instantly in the plain `npm run dev` workflow, which is exactly why that
  workflow keeps its own separate `.env.development` value instead of sharing this one.
