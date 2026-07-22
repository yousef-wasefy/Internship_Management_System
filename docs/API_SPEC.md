# API Specification

Base URL (local development): `http://localhost:5053/api`

Interactive documentation (Swagger UI): `http://localhost:5053/swagger`

**Authentication:** none yet — every endpoint below is currently public. JWT-based
authentication and role-based authorization are added in Phase 6, at which point this
document will note which endpoints require a token and which role(s) can call them.

---

## Internships

Backed by `InternshipsController` → `IInternshipService` → `AppDbContext`.

> **Temporary note (removed after Phase 8):** every `InternshipPost` created right now is
> automatically assigned to a single seeded placeholder company (see
> `Data/SeedData.cs`) — there is no concept of "the logged-in company" yet, since
> authentication doesn't exist until Phase 6. `POST`/`PUT` do not accept a `companyId`;
> the server decides it. Phase 8 replaces this with the real logged-in company and adds
> a publishing workflow (`Draft` → `Open` → `Closed`/`Cancelled`) with ownership checks.

### `GET /api/internships`

Returns a summary list of all internship posts (no filtering yet — added in Phase 12).

**Response `200 OK`** — `InternshipListDto[]`
```json
[
  {
    "id": 2,
    "title": "Backend Developer Intern",
    "location": "Cairo, Egypt",
    "workMode": "Remote",
    "applicationDeadline": "2026-12-31T00:00:00Z",
    "status": "Draft",
    "companyName": "Placeholder Company (temporary - see Phase 8)"
  }
]
```

### `GET /api/internships/{id}`

Returns the full details of a single internship post.

**Response `200 OK`** — `InternshipDetailsDto`
```json
{
  "id": 2,
  "title": "Backend Developer Intern",
  "description": "Work on our ASP.NET Core API",
  "requirements": "C#, SQL basics",
  "responsibilities": "Build REST endpoints",
  "location": "Cairo, Egypt",
  "workMode": "Remote",
  "duration": "3 months",
  "applicationDeadline": "2026-12-31T00:00:00Z",
  "status": "Draft",
  "companyName": "Placeholder Company (temporary - see Phase 8)",
  "createdAt": "2026-07-22T16:12:56.972Z",
  "updatedAt": "2026-07-22T16:12:56.972Z"
}
```
**Response `404 Not Found`** — no post exists with that id.

### `POST /api/internships`

Creates a new internship post. Always starts as `Status: "Draft"`.

**Request body** — `CreateInternshipDto`
```json
{
  "title": "Backend Developer Intern",
  "description": "Work on our ASP.NET Core API",
  "requirements": "C#, SQL basics",
  "responsibilities": "Build REST endpoints",
  "location": "Cairo, Egypt",
  "workMode": "Remote",
  "duration": "3 months",
  "applicationDeadline": "2026-12-31T00:00:00Z"
}
```
`title` is required; every other field is optional. `workMode` must be one of
`"Onsite"`, `"Remote"`, `"Hybrid"`. `applicationDeadline` should be an ISO-8601 date-time;
if no timezone offset is given, it's treated as UTC.

**Response `201 Created`** — `InternshipDetailsDto` (see shape above), with a `Location`
response header pointing at `GET /api/internships/{id}`.

### `PUT /api/internships/{id}`

Replaces the editable fields of an existing internship post. Does **not** change
`Status` — that's handled by dedicated endpoints in Phase 8.

**Request body** — `UpdateInternshipDto` (same shape as `CreateInternshipDto`).

**Response `204 No Content`** — update succeeded.
**Response `404 Not Found`** — no post exists with that id.

### `DELETE /api/internships/{id}`

Permanently deletes an internship post.

**Response `204 No Content`** — deleted.
**Response `404 Not Found`** — no post exists with that id.

---

## Not Yet Implemented

Endpoints named in `docs/PHASES.md` §11 but not built yet, added in later phases:
- `Auth` (Phase 6), `Students`/`Companies` profile endpoints (Phase 7)
- `PATCH /api/internships/{id}/open` / `.../close` (Phase 8)
- `Applications` endpoints (Phase 9–10)
- `Admin` endpoints (Phase 11)
