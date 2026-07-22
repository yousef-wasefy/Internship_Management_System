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
