# Deployment (Phase 17)

How to put a real, publicly reachable copy of this system online via
[Render](https://render.com), using the `render.yaml` Blueprint at the repo root. Render
builds the same Dockerfiles from Phase 16 — no new backend or frontend code exists
just for deployment beyond what `docs/DECISIONS.md` D22 explains.

## 1. Prerequisites

- This repo pushed to GitHub (already done — `origin` points at
  `github.com/yousef-wasefy/Internship_Management_System`, `main` up to date).
- A Render account. Account creation isn't something anyone but you can do — sign up at
  [render.com](https://render.com), preferably **using "Sign up with GitHub"** so the
  next step (connecting the repo) needs no extra setup.

## 2. Deploy the Blueprint

1. In the Render dashboard: **New** → **Blueprint**.
2. Connect (or select) this GitHub repo. Render will find `render.yaml` at the root
   automatically.
3. Render shows a preview of the three resources it's about to create — a Postgres
   database and two web services (`internship-mgmt-api`, `internship-mgmt-frontend`).
   Click **Apply**.
4. Render builds both Docker images and provisions the database. The backend's own
   startup (Phase 16's `Database.MigrateAsync()` + admin seed) creates the schema and a
   working admin login automatically — no manual database setup step exists.
5. First build typically takes several minutes (free-tier compute is intentionally
   modest). Watch the build logs in the Render dashboard for each service.

## 3. After the first deploy — check the two guessed URLs

`render.yaml` has to guess each service's public URL before Render actually assigns
one (there's no way to reference another service's real public hostname from inside a
Blueprint — see `docs/DECISIONS.md` D22). If Render had to rename either service
(e.g. `internship-mgmt-api` was already taken by someone else's app), two environment
variables will be wrong:

| Service | Env var | Should equal |
|---|---|---|
| `internship-mgmt-api` | `Cors__AllowedOrigins` | The frontend service's actual `.onrender.com` URL |
| `internship-mgmt-frontend` | `API_BASE_URL` | The backend service's actual `.onrender.com` URL, plus `/api` |

Fix these from each service's **Environment** tab in the Render dashboard if needed,
then use **Manual Deploy** to redeploy just that one service (changing `API_BASE_URL`
only needs the frontend container to restart with the new value — Phase 17's runtime
env-config pattern means this does **not** require rebuilding the image, see D22).

## 4. Verify the live deployment

- Open the frontend's URL — the public internship listing should load (empty at
  first, since this is a brand new database).
- Open the backend's URL + `/swagger` — the same interactive API docs used throughout
  local development.
- Log in as the seeded admin: `admin@internship-system.local` / `Admin@12345` (same
  dev-only credential documented in `backend/.../Data/SeedData.cs` since Phase 6 —
  consider changing this password once deployed, since the seed is public in this
  repo's source).
- Run through the smoke test in `docs/PHASES.md`'s "End-to-End Smoke Test" section
  against the live URLs.

## 5. Known limitations of the free tier

- **The free Postgres database expires 30 days after creation** (plus a 14-day grace
  period before deletion) unless upgraded to a paid plan. Fine for an initial public
  demo; revisit before relying on this for anything longer-lived.
- **Free web services spin down after a period of inactivity** and take on the order of
  30–60 seconds to spin back up on the next request — the first load after a while idle
  will feel slow. Later requests are normal speed until it idles again.
- Neither limitation requires any code change to work around — they're Render account/
  plan decisions, not application bugs.

## 6. Redeploying after future changes

Render's Blueprint services auto-deploy on every push to `main` by default (visible/
configurable per-service in the dashboard). No extra step is needed beyond the normal
`git push` you'd already do.
