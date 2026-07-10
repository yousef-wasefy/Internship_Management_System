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
