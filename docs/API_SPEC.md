# API Specification

Base URL (local development): `http://localhost:5053/api`

Interactive documentation (Swagger UI): `http://localhost:5053/swagger`

**Authentication:** JWT bearer tokens, added in Phase 6. Register or log in to get a
token, then send it as `Authorization: Bearer <token>` on protected endpoints. In Swagger,
click **Authorize** and paste just the token (no `Bearer ` prefix needed). Tokens expire
after 60 minutes.

**CORS (Phase 13):** for local development (`npm run dev` + `dotnet run`), the API
allows cross-origin requests from `http://localhost:5173` only — the Vite dev server the
`frontend/` React app runs on (see `docs/DECISIONS.md` D18). Any other origin's
browser-based requests are blocked by the browser itself (not by the API rejecting
them); Swagger and Postman aren't affected, since CORS is a browser-enforced
restriction. **This CORS policy is unused by the Docker Compose setup** (Phase 16) —
there, nginx reverse-proxies the frontend and the API behind one shared origin, so the
browser never makes a cross-origin request in the first place. See `docs/DECISIONS.md`
D21 and the root `docker-compose.yml` for how that setup runs.

---

## Error Response Format

Every error response in this API — validation failures, business rule violations,
authentication/authorization failures, "not found," and unexpected server errors —
returns the same shape: **[RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457)**
(`application/problem+json`), via ASP.NET Core's built-in `AddProblemDetails()` plus a
custom authorization result handler (Phase 12 — see `docs/DECISIONS.md` D17).

**Business rule / not found / forbidden / unauthorized** (`type`, `title`, `status`,
`detail`, `traceId`):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Internship not found.",
  "traceId": "00-d44386b22fffa6669bb229cdd6f78fb7-b9844d3802134420-00"
}
```

**DTO validation failures** (automatic, from `[Required]`/`[EmailAddress]`/`[StringLength]`/
etc. on the request DTOs) — same shape, plus a field-by-field `errors` dictionary instead
of a single `detail`:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."],
    "Password": ["The field Password must be a string or array type with a minimum length of '8'."]
  },
  "traceId": "00-5877bae8ef3bc52652721f0469427f09-6ad6ab7f119cf2f7-00"
}
```

**Unexpected server errors** (`500`, from `Middleware/GlobalExceptionHandler.cs`) — same
shape, deliberately **no `detail`**: the real exception is logged server-side, but never
echoed to the client (an information-disclosure risk otherwise):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An unexpected error occurred.",
  "status": 500
}
```

Per-endpoint sections below only note the **status code** and, where relevant, the exact
`detail` message(s) a business rule can produce — the envelope shape is always the one
above.

---

## Validation

Every request DTO validates its fields via .NET's `DataAnnotations`
(`[Required]`, `[EmailAddress]`, `[StringLength]`, `[MinLength]`, `[Url]`) — enforced
automatically by `[ApiController]` before a request ever reaches a controller action.
Failures return `400` in the validation shape above. Notable rules:
- **Emails** must be valid email format (`RegisterStudentDto`, `RegisterCompanyDto`, `LoginDto`).
- **Passwords** need at least 8 characters **at registration** — `LoginDto.Password` only
  requires *some* value be sent (a too-short guess still just fails normally via BCrypt).
- **`FullName`/`CompanyName`/`Title`** are required, capped at 200 characters.
- **URL fields** (`CVUrl`, `LinkedInUrl`, `GitHubUrl`, `WebsiteUrl`) must be well-formed
  URLs *if provided* — all are optional.
- **`CreateInternshipDto`/`UpdateInternshipDto.ApplicationDeadline`** has **no** "must be
  in the future" validation at the DTO level — a Draft can have its deadline decided
  later. That specific rule is a *business* rule, enforced only when *opening* a post
  (see Internships → Publishing rules below) — DTO validation checks request *shape*,
  not business *permission*.

---

## Auth

Backed by `AuthController` → `IAuthService` → `AppDbContext`. All four endpoints are
public (no token required to call them) — `register-*` and `login` hand out a token,
`me` requires one.

### `POST /api/auth/register-student`

Creates a new **Student** account (a `User` + a `StudentProfile`) and immediately returns
a token, the same as logging in right after.

**Request body** — `RegisterStudentDto`
```json
{ "email": "sara@example.com", "password": "Passw0rd!", "fullName": "Sara Ahmed" }
```
**Response `200 OK`** — `AuthResponseDto`
```json
{
  "token": "eyJhbGciOi...",
  "expiresAt": "2026-07-23T14:26:06.968Z",
  "email": "sara@example.com",
  "role": "Student"
}
```
**Response `400 Bad Request`** — DTO validation failure (see Validation above).
**Response `409 Conflict`** — `detail: "An account with this email already exists."`

### `POST /api/auth/register-company`

Creates a new **Company** account (a `User` + a `CompanyProfile`, starting unapproved —
REQUIREMENTS.md CO-3). Same response shape as above, with `role: "Company"`.

**Request body** — `RegisterCompanyDto`
```json
{ "email": "hr@acme.com", "password": "Passw0rd!", "companyName": "Acme Corp" }
```
**Response `200 OK`** — `AuthResponseDto`. **Response `400`/`409`** — same as above.

### `POST /api/auth/login`

**Request body** — `LoginDto`
```json
{ "email": "sara@example.com", "password": "Passw0rd!" }
```
**Response `200 OK`** — `AuthResponseDto` (same shape as register).
**Response `401 Unauthorized`** — `detail: "Invalid email or password."` — wrong
password, unknown email, **or** a disabled account all produce this identical message;
the API never reveals *which* part was wrong.

### `GET /api/auth/me`

Returns the identity of whoever the bearer token belongs to. **Requires a token.**

**Response `200 OK`** — `CurrentUserDto`
```json
{ "id": 6, "email": "sara@example.com", "role": "Student" }
```
**Response `401 Unauthorized`** — missing, expired, or invalid token, **or** the token
belongs to a now-disabled account (re-checked against the database on every call to this
specific endpoint — see the Admin section's note on disabled-user tokens).

---

## Students

Backed by `StudentsController` → `IStudentService` → `AppDbContext`. Both endpoints
require a valid token with the **Student** role.

### `GET /api/students/me`

Returns the logged-in student's own profile.

**Response `200 OK`** — `StudentProfileDto`
```json
{
  "id": 5,
  "email": "omar@example.com",
  "fullName": "Omar Khaled",
  "university": "Cairo University",
  "faculty": "Engineering",
  "major": "Computer Science",
  "academicYear": "3rd Year",
  "skills": "C#, SQL, React",
  "cvUrl": "https://example.com/cv.pdf",
  "linkedInUrl": "https://linkedin.com/in/omark",
  "gitHubUrl": "https://github.com/omark",
  "createdAt": "2026-07-24T19:52:30.189Z",
  "updatedAt": "2026-07-24T19:52:44.800Z"
}
```
**Response `401 Unauthorized`** — no/invalid token. **Response `403 Forbidden`** — a
valid token that isn't a Student (e.g. a Company).

### `PUT /api/students/me`

Updates the logged-in student's own profile. `fullName` is required; everything else is
optional (see Validation above for field-level rules).

**Request body** — `UpdateStudentProfileDto` (same fields as the DTO above, minus `id`,
`email`, and the timestamps — those aren't editable through this endpoint).

**Response `204 No Content`** — updated.
**Response `400 Bad Request`** — DTO validation failure.
**Response `401`/`403`** — same rules as `GET`.

---

## Companies

Backed by `CompaniesController` → `ICompanyService`/`IInternshipService`/
`IApplicationService` → `AppDbContext`. Every endpoint requires a valid token with the
**Company** role. (One more endpoint on this controller, `GET /me/applications`, is
documented under "Company applicant views" further below, grouped with the other
applicant-review endpoints rather than repeated here.)

### `GET /api/companies/me`

Returns the logged-in company's own profile, including its approval status.

**Response `200 OK`** — `CompanyProfileDto`
```json
{
  "id": 7,
  "email": "hr@acme.com",
  "companyName": "Acme Corp",
  "industry": "Software",
  "websiteUrl": "https://acme.example.com",
  "description": "We build things",
  "location": "Cairo, Egypt",
  "isApproved": false,
  "createdAt": "2026-07-24T19:52:31.735Z",
  "updatedAt": "2026-07-24T19:52:44.901Z"
}
```
**Response `401`/`403`** — same rules as the Students endpoints, for the Company role.

### `PUT /api/companies/me`

Updates the logged-in company's own profile. `companyName` is required; everything else
is optional. **Cannot change `isApproved`** — that field isn't part of the request DTO at
all; only an admin can approve a company (see below).

**Request body** — `UpdateCompanyProfileDto`.

**Response `204 No Content`** — updated.
**Response `400 Bad Request`** — DTO validation failure.
**Response `401`/`403`** — same rules as `GET`.

### `GET /api/companies/me/internships`

Returns internship posts owned by the logged-in company, regardless of status — added in
Phase 8, since the public `GET /api/internships` listing only shows `Open` posts, so a
company needs its own way to see (and find the id of) its drafts and closed posts.

**Query parameters:**
- `status` *(optional, Phase 12)* — filter to exactly one status (e.g. `?status=Draft`).
  Omit to get every status. Safe to expose here (unlike on the public listing) since the
  caller already sees every status regardless.

**Response `200 OK`** — `InternshipListDto[]` (same shape as the public listing, **not
paginated** — a company's own post count is expected to stay small; pagination was only
added to the public listing, see Internships below).
**Response `401`/`403`** — same rules as the other Companies endpoints.

### `GET /api/companies/me/internships/{id}`

*(Phase 14)* One of the company's own internship posts, full details, regardless of
status. Added specifically because `GET /api/internships/{id}` (the public endpoint)
only ever returns `Open` posts — a company had no way to fetch its own Draft or Closed
post's full details (e.g. to pre-fill an edit form) until this endpoint existed. Same
data shape as the public detail endpoint; the only difference is which posts it can see.

**Response `200 OK`** — `InternshipDetailsDto` (same shape as `GET /api/internships/{id}`).
**Response `404 Not Found`** — no post with this id exists.
**Response `403 Forbidden`** — the post exists but belongs to a different company.
**Response `401`** — no/invalid token, or the token isn't a Company.

---

## Admin

Backed by `AdminController` → `IAdminService` → `AppDbContext`. Every endpoint requires a
valid token with the **Admin** role.

### `GET /api/admin/dashboard`

Platform-wide statistics.

**Response `200 OK`** — `AdminDashboardDto`
```json
{
  "totalStudents": 12,
  "totalCompanies": 14,
  "pendingCompanies": 1,
  "totalInternships": 3,
  "openInternships": 3,
  "totalApplications": 3,
  "acceptedApplications": 1,
  "rejectedApplications": 1
}
```

### `GET /api/admin/companies/pending`

Companies that are neither approved nor already rejected — the admin's review queue.

**Response `200 OK`** — `CompanyProfileDto[]` (same shape as `GET /api/companies/me`).

### `PATCH /api/admin/companies/{id}/approve`

Approves a company by its `CompanyProfile` id (not the company's `User` id). Sets
`IsApproved = true`. Once approved, it drops off the pending list.

**Response `200 OK`** — the updated `CompanyProfileDto` (`isApproved: true`).
**Response `404 Not Found`** — `detail: "Company not found."`

### `PATCH /api/admin/companies/{id}/reject`

Rejects a company. The schema has no separate "Rejected" state (`CompanyProfile` only
has a boolean `IsApproved`), so rejecting keeps `IsApproved` false **and disables the
underlying account** (`User.IsDisabled = true`) — a rejected company can no longer log in
at all, and naturally drops off the pending list. See `docs/DECISIONS.md` D16.

**Response `200 OK`** — the updated `CompanyProfileDto` (`isApproved: false`).
**Response `404 Not Found`** — `detail: "Company not found."`

### `GET /api/admin/users`

Every user on the platform, most recently created first.

**Response `200 OK`** — `AdminUserDto[]`
```json
[
  {
    "id": 42,
    "email": "sara@example.com",
    "displayName": "Sara Ahmed",
    "role": "Student",
    "isDisabled": false,
    "createdAt": "2026-08-14T20:00:00.000Z"
  }
]
```
`displayName` is the student's full name, the company's name, or `null` for an Admin
account — resolved from whichever profile (if any) belongs to the user.

### `PATCH /api/admin/users/{id}/disable`

Disables any user by their `User` id — blocks future logins (`AuthService.LoginAsync`
rejects a disabled account, same generic `401` every failed login uses). **Does not
revoke tokens already issued** — see the note below.

**Response `200 OK`** — the updated `AdminUserDto` (`isDisabled: true`).
**Response `404 Not Found`** — `detail: "User not found."`

> **Known limitation, not a bug:** JWTs are stateless and validated purely
> cryptographically — disabling a user blocks their *next login*, but any token issued
> *before* the disable action remains valid (accepted by `[Authorize]`) until it
> naturally expires (60 minutes, per `Jwt:ExpiryMinutes`). The one exception is
> `GET /api/auth/me`, which re-checks `IsDisabled` against the database on every call and
> will correctly return `401` even for an already-issued token. Every other endpoint does
> not perform this per-request check. A real revocation system (a token blocklist, or
> short-lived tokens with refresh) is future work, not built in this phase — see D16.

---

## Internships

Backed by `InternshipsController` → `IInternshipService` → `AppDbContext`.

**Authorization:** `GET` endpoints are public. `POST`/`PUT`/`DELETE`/`PATCH .../open`/
`PATCH .../close` all require a valid token with the **Company** role, *and* — for
everything except `POST` — that the token belongs to the company that owns the specific
post being acted on (Phase 8; see the Ownership rules below).

**Status workflow:** `Draft` → `Open` → `Closed`. `Cancelled` exists as a status but there
is currently no endpoint anywhere in the API that sets it — a known, documented gap (not
a Phase 8 bug; no phase in the plan adds a cancel action).

**Ownership rules** (`OperationResult.Forbidden` → HTTP `403`):
- A company can `PUT`/`DELETE`/open/close only posts it owns. Acting on another
  company's post → `403 Forbidden` (verified live: a second company attempting to close
  or edit the first company's post both correctly returned 403).
- `POST` (create) has no ownership check to make — the new post is always owned by
  whichever company is logged in (resolved via the JWT, not client-supplied).

**Publishing rules** (checked by `PATCH .../open`, `OperationResult.ValidationFailed` →
HTTP `400`, `detail` set to the specific message):
1. The owning company must be **approved** (`CompanyProfile.IsApproved == true`) —
   otherwise: `"Your company must be approved by an admin before you can open internship posts."`
2. `Title` and `Description` must both be non-empty — otherwise:
   `"Title and description are required before opening an internship."`
3. `ApplicationDeadline` must be in the future — otherwise:
   `"The application deadline must be in the future to open this internship."`
4. A `Cancelled` post can never be reopened — otherwise:
   `"A cancelled internship cannot be reopened."`

**Closing rules** (`PATCH .../close`): only a post whose current status is `Open` can be
closed — otherwise `400` with `detail: "Only an open internship can be closed."`

### `GET /api/internships`

Public, paginated listing of **Open** internship posts (Phase 8: students should never
see another company's drafts), with optional filtering and search (Phase 12).

**Query parameters** (`InternshipQueryParameters`, all optional):
- `page` — 1-based page number. Default `1`.
- `pageSize` — items per page. Default `10`, clamped to a maximum of `50`.
- `location` — case-insensitive partial match against `Location`.
- `workMode` — exact match: `Onsite`, `Remote`, or `Hybrid`.
- `search` — case-insensitive partial match against `Title` only.

No `status` filter here on purpose — the listing is hard-restricted to `Open` regardless
of input; a status filter would either be a no-op or misleadingly imply other statuses
are reachable. (A company's own listing, above, supports filtering by status instead,
since it's safe there.)

**Example:** `GET /api/internships?location=Cairo&workMode=Remote&search=backend&page=1&pageSize=10`

**Response `200 OK`** — `PagedResult<InternshipListDto>`
```json
{
  "items": [
    {
      "id": 18,
      "title": "Backend Intern",
      "location": "Cairo, Egypt",
      "workMode": "Remote",
      "applicationDeadline": "2026-12-31T00:00:00Z",
      "status": "Open",
      "companyName": "Phase12 Co"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

> **Note:** this response shape changed in Phase 12 — before this phase, `GET /api/internships`
> returned a bare `InternshipListDto[]` array. Any client written against the Phase
> 5–11 shape needs updating to read `.items` instead of using the response directly as
> an array.

### `GET /api/internships/{id}`

Returns the full details of a single internship post — **only if it's `Open`** (Phase 8;
a direct id lookup can no longer reveal an unpublished draft). Public, no token required.

**Response `200 OK`** — `InternshipDetailsDto`
```json
{
  "id": 7,
  "title": "Backend Intern",
  "description": "Work on APIs",
  "requirements": null,
  "responsibilities": null,
  "location": null,
  "workMode": "Remote",
  "duration": null,
  "applicationDeadline": "2026-12-31T00:00:00Z",
  "status": "Open",
  "companyName": "Company A",
  "createdAt": "2026-07-27T20:39:17.209Z",
  "updatedAt": "2026-07-27T20:39:35.448Z"
}
```
**Response `404 Not Found`** — no post with that id, **or it exists but isn't `Open`**
(the two cases are indistinguishable on purpose — a company's own listing, above, is
where the real status is visible).

### `POST /api/internships`

Creates a new internship post, owned by the logged-in company. Always starts as
`Status: "Draft"` — use `PATCH .../open` to publish it.

**Request body** — `CreateInternshipDto`
```json
{
  "title": "Backend Intern",
  "description": "Work on APIs",
  "workMode": "Remote",
  "applicationDeadline": "2026-12-31T00:00:00Z"
}
```
`title` is required (see Validation above); every other field is optional (though
`description` becomes effectively required before the post can be opened — see
Publishing rules above). `workMode` must be one of `"Onsite"`, `"Remote"`, `"Hybrid"`.
`applicationDeadline` should be an ISO-8601 date-time; if no timezone offset is given, it
is treated as UTC.

**Response `201 Created`** — `InternshipDetailsDto`, `Location` header → `GET /api/internships/{id}`
(note: that `GET` will 404 until the post is opened — it's a `Draft`).
**Response `400 Bad Request`** — DTO validation failure.
**Response `401`/`403`** — no/invalid token, or a valid token that isn't a Company.

### `PUT /api/internships/{id}`

Replaces the editable fields of an existing internship post. Does **not** change
`Status`. **Owner only.**

**Request body** — `UpdateInternshipDto` (same shape as `CreateInternshipDto`).

**Response `204 No Content`** — updated.
**Response `400 Bad Request`** — DTO validation failure.
**Response `404 Not Found`** — `detail: "Internship not found."`
**Response `403 Forbidden`** — `detail: "You do not own this internship post."`
**Response `401 Unauthorized`** — no/invalid token.

### `DELETE /api/internships/{id}`

Permanently deletes an internship post. **Owner only.**

**Response `204 No Content`** — deleted.
**Response `404`/`403`/`401`** — same rules as `PUT` above.

### `PATCH /api/internships/{id}/open`

Publishes a `Draft` (or reopens a `Closed`) post, subject to the Publishing rules above.
**Owner only.**

**Response `200 OK`** — the updated `InternshipDetailsDto` (`status: "Open"`).
**Response `400 Bad Request`** — one of the four Publishing rule messages above, as `detail`.
**Response `404`/`403`/`401`** — same rules as `PUT`.

### `PATCH /api/internships/{id}/close`

Closes an `Open` post. **Owner only.**

**Response `200 OK`** — the updated `InternshipDetailsDto` (`status: "Closed"`).
**Response `400 Bad Request`** — `detail: "Only an open internship can be closed."`
**Response `404`/`403`/`401`** — same rules as `PUT`.

### `POST /api/internships/{id}/apply`

Applies to an internship as the logged-in student. Requires a valid token with the
**Student** role.

**Request body** — `ApplyToInternshipDto` (both fields optional)
```json
{ "coverLetter": "I'm interested!", "cvUrl": "https://example.com/cv.pdf" }
```
**Response `201 Created`** — the new `ApplicationDto` (`status: "Pending"`). No `Location`
header — there's no `GET /api/applications/{id}` endpoint yet to point at (see
"Not Yet Implemented" below); use `GET /api/applications/my` instead.
**Response `400 Bad Request`** — DTO validation failure, or one of:
  - `"This internship is not open for applications."` — the post is `Draft`, `Closed`,
    or `Cancelled` (deliberately the same message for all three — which one it is isn't
    the student's business).
  - `"The application deadline for this internship has passed."` — checked separately
    from `Status`, since nothing automatically closes a post once its deadline passes.
  - `"You have already applied to this internship."` — enforced twice: a friendly
    pre-check, backed by the database's own composite unique constraint (Phase 4) as the
    final word, in case of a race between two near-simultaneous requests.
**Response `404 Not Found`** — no internship with that id.
**Response `401`/`403`** — no/invalid token, or a valid token that isn't a Student.

### `GET /api/internships/{id}/applications`

Returns every applicant for one specific internship, from the owning company's
perspective. **Owner only.**

**Response `200 OK`** — `ApplicantDto[]` (see shape under Applications below).
**Response `404`/`403`/`401`** — same rules as `PUT /api/internships/{id}`.

---

## Applications

Backed by `ApplicationsController` → `IApplicationService` → `AppDbContext`.
`GET /my` and `PATCH .../withdraw` require the **Student** role; `PATCH .../status`
requires the **Company** role — this controller has no controller-level `[Authorize]`,
since (unlike `StudentsController`/`CompaniesController`) it mixes roles across actions.

### `GET /api/applications/my`

Returns every application the logged-in student has ever submitted, most recent first,
regardless of status. **Student role.**

**Response `200 OK`** — `ApplicationDto[]`
```json
[
  {
    "id": 3,
    "internshipPostId": 11,
    "internshipTitle": "Internship X",
    "companyName": "Company C",
    "coverLetter": "I'm interested!",
    "cvUrl": null,
    "status": "Pending",
    "appliedAt": "2026-08-07T18:13:14.071Z",
    "updatedAt": "2026-08-07T18:13:14.071Z",
    "reviewedAt": null,
    "companyNotes": null
  }
]
```

### `PATCH /api/applications/{id}/withdraw`

Withdraws the logged-in student's own application. **Student role, owner only, and only
while `Pending`.**

**Response `200 OK`** — the updated `ApplicationDto` (`status: "Withdrawn"`).
**Response `404 Not Found`** — `detail: "Application not found."`
**Response `403 Forbidden`** — `detail: "This is not your application."`
**Response `400 Bad Request`** — `detail: "Only a pending application can be withdrawn."`
— e.g. it was already withdrawn, or a company already shortlisted/accepted/rejected it.
**Response `401 Unauthorized`** — no/invalid token.

### `PATCH /api/applications/{id}/status`

Lets the owning company shortlist, accept, or reject an application. **Company role,
owner of the application's internship only.**

**Request body** — `UpdateApplicationStatusDto`
```json
{ "status": "Shortlisted", "companyNotes": "Strong candidate, schedule interview" }
```
`status` must be `"Shortlisted"`, `"Accepted"`, or `"Rejected"` — not `"Pending"` (the
default) or `"Withdrawn"` (student-only, via the endpoint above). `companyNotes` is
optional; **if omitted, any existing note is left untouched** rather than being cleared
(so shortlisting with a note, then later accepting without resupplying it, doesn't erase
the note).

**Response `200 OK`** — the updated `ApplicantDto`:
```json
{
  "id": 6,
  "internshipPostId": 16,
  "internshipTitle": "Data Intern",
  "studentFullName": "Applicant One",
  "studentEmail": "p10student1@example.com",
  "studentUniversity": null,
  "studentMajor": null,
  "studentSkills": null,
  "studentLinkedInUrl": null,
  "studentGitHubUrl": null,
  "coverLetter": "Applicant 1 cover letter",
  "cvUrl": null,
  "status": "Shortlisted",
  "appliedAt": "2026-08-14T20:05:00.000Z",
  "updatedAt": "2026-08-14T20:06:00.000Z",
  "reviewedAt": "2026-08-14T20:06:00.000Z",
  "companyNotes": "Strong candidate, schedule interview"
}
```
**Response `404 Not Found`** — `detail: "Application not found."`
**Response `403 Forbidden`** — `detail: "You do not own the internship this application is for."`
**Response `400 Bad Request`** — one of:
  - `"A withdrawn application cannot be reviewed."` — checked before the requested
    status value, since a withdrawn application is off-limits regardless of what it was
    being changed to (REQUIREMENTS.md §4.2 rule 5).
  - `"Status must be Shortlisted, Accepted, or Rejected."`
**Response `401 Unauthorized`** — no/invalid token.

---

## Company applicant views

Two more endpoints, added alongside the status update above:

- **`GET /api/companies/me/applications`** — every applicant across *all* of the
  logged-in company's internships, most recent first. **Company role.**
  Response: `200 OK` — `ApplicantDto[]`.
- **`GET /api/internships/{id}/applications`** — documented above, under Internships.

---

## Not Yet Implemented

Endpoints named in the original project brief but not built yet, added in later phases:
- `GET /api/applications/{id}` (a single-application detail view — `GET /my` and the two
  company-facing listing endpoints have covered every need so far)
- A way to re-enable a disabled user, or "un-reject" a company — only the
  forward/disabling actions exist (Phase 11 didn't name a reverse action; a direct
  database fix is the only way today if one is ever needed)
