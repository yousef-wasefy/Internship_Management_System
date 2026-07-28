# API Specification

Base URL (local development): `http://localhost:5053/api`

Interactive documentation (Swagger UI): `http://localhost:5053/swagger`

**Authentication:** JWT bearer tokens, added in Phase 6. Register or log in to get a
token, then send it as `Authorization: Bearer <token>` on protected endpoints. In Swagger,
click **Authorize** and paste just the token (no `Bearer ` prefix needed). Tokens expire
after 60 minutes.

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
**Response `409 Conflict`** — an account with this email already exists.

### `POST /api/auth/register-company`

Creates a new **Company** account (a `User` + a `CompanyProfile`, starting unapproved —
REQUIREMENTS.md CO-3). Same response shape as above, with `role: "Company"`.

**Request body** — `RegisterCompanyDto`
```json
{ "email": "hr@acme.com", "password": "Passw0rd!", "companyName": "Acme Corp" }
```
**Response `200 OK`** — `AuthResponseDto` · **Response `409 Conflict`** — email taken.

### `POST /api/auth/login`

**Request body** — `LoginDto`
```json
{ "email": "sara@example.com", "password": "Passw0rd!" }
```
**Response `200 OK`** — `AuthResponseDto` (same shape as register).
**Response `401 Unauthorized`** — wrong password, unknown email, or a disabled account.
The message is identical in every case (`"Invalid email or password."`) — the API never
reveals *which* part was wrong.

### `GET /api/auth/me`

Returns the identity of whoever the bearer token belongs to. **Requires a token.**

**Response `200 OK`** — `CurrentUserDto`
```json
{ "id": 6, "email": "sara@example.com", "role": "Student" }
```
**Response `401 Unauthorized`** — missing, expired, or invalid token.

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
optional.

**Request body** — `UpdateStudentProfileDto` (same fields as the DTO above, minus `id`,
`email`, and the timestamps — those aren't editable through this endpoint).

**Response `204 No Content`** — updated. **Response `401`/`403`** — same rules as `GET`.

---

## Companies

Backed by `CompaniesController` → `ICompanyService` → `AppDbContext`. Both endpoints
require a valid token with the **Company** role.

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

**Response `204 No Content`** — updated. **Response `401`/`403`** — same rules as `GET`.

### `GET /api/companies/me/internships`

Returns **every** internship post owned by the logged-in company, regardless of status
(`Draft`/`Open`/`Closed`/`Cancelled`) — added in Phase 8, since the public
`GET /api/internships` listing only shows `Open` posts, so a company needs its own way to
see (and find the id of) its drafts and closed posts.

**Response `200 OK`** — `InternshipListDto[]` (same shape as the public listing).
**Response `401`/`403`** — same rules as the other Companies endpoints.

---

## Admin *(stub — full module in Phase 11)*

Backed by `AdminController` → `ICompanyService`. Requires a valid token with the
**Admin** role.

### `PATCH /api/admin/companies/{id}/approve`

Approves a company by its `CompanyProfile` id (not the company's `User` id). Sets
`IsApproved = true`.

**Response `200 OK`** — the updated `CompanyProfileDto` (`isApproved: true`).
**Response `404 Not Found`** — no company profile with that id.
**Response `401`/`403`** — no/invalid token, or a valid token that isn't an Admin.

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
HTTP `400` with a `{ "message": "..." }` body):
1. The owning company must be **approved** (`CompanyProfile.IsApproved == true`) —
   otherwise: `"Your company must be approved by an admin before you can open internship posts."`
2. `Title` and `Description` must both be non-empty — otherwise:
   `"Title and description are required before opening an internship."`
3. `ApplicationDeadline` must be in the future — otherwise:
   `"The application deadline must be in the future to open this internship."`
4. A `Cancelled` post can never be reopened — otherwise:
   `"A cancelled internship cannot be reopened."`

**Closing rules** (`PATCH .../close`): only a post whose current status is `Open` can be
closed — otherwise `400` with `"Only an open internship can be closed."`

### `GET /api/internships`

Returns a summary list of internship posts — **only `Open` ones** (Phase 8; students
should never see another company's drafts). Public, no token required.

**Response `200 OK`** — `InternshipListDto[]`
```json
[
  {
    "id": 7,
    "title": "Backend Intern",
    "location": null,
    "workMode": "Remote",
    "applicationDeadline": "2026-12-31T00:00:00Z",
    "status": "Open",
    "companyName": "Company A"
  }
]
```

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
(the two cases are indistinguishable on purpose — a company's own listing, below, is
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
`title` is required; every other field is optional (though `description` becomes
effectively required before the post can be opened — see Publishing rules above).
`workMode` must be one of `"Onsite"`, `"Remote"`, `"Hybrid"`. `applicationDeadline`
should be an ISO-8601 date-time; if no timezone offset is given, it's treated as UTC.

**Response `201 Created`** — `InternshipDetailsDto`, `Location` header → `GET /api/internships/{id}`
(note: that `GET` will 404 until the post is opened — it's a `Draft`).
**Response `401`/`403`** — no/invalid token, or a valid token that isn't a Company.

### `PUT /api/internships/{id}`

Replaces the editable fields of an existing internship post. Does **not** change
`Status`. **Owner only.**

**Request body** — `UpdateInternshipDto` (same shape as `CreateInternshipDto`).

**Response `204 No Content`** — updated.
**Response `404 Not Found`** — no post with that id.
**Response `403 Forbidden`** — valid Company token, but not the post's owner.
**Response `401 Unauthorized`** — no/invalid token.

### `DELETE /api/internships/{id}`

Permanently deletes an internship post. **Owner only.**

**Response `204 No Content`** — deleted.
**Response `404`/`403`/`401`** — same rules as `PUT` above.

### `PATCH /api/internships/{id}/open`

Publishes a `Draft` (or reopens a `Closed`) post, subject to the Publishing rules above.
**Owner only.**

**Response `200 OK`** — the updated `InternshipDetailsDto` (`status: "Open"`).
**Response `400 Bad Request`** — `{ "message": "..." }`, one of the four Publishing rule
messages above.
**Response `404`/`403`/`401`** — same rules as `PUT`.

### `PATCH /api/internships/{id}/close`

Closes an `Open` post. **Owner only.**

**Response `200 OK`** — the updated `InternshipDetailsDto` (`status: "Closed"`).
**Response `400 Bad Request`** — `{ "message": "Only an open internship can be closed." }`.
**Response `404`/`403`/`401`** — same rules as `PUT`.

---

## Not Yet Implemented

Endpoints named in `docs/PHASES.md` §11 but not built yet, added in later phases:
- `Applications` endpoints (Phase 9–10)
- The rest of the `Admin` module: dashboard, pending-companies list, reject, user
  management (Phase 11 — only the approve action exists so far)
