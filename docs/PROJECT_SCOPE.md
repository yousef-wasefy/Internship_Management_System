# Project Scope

## Project Name

**Internship Management System**

## Goal

Build a realistic full-stack web application that simulates how **students** apply for
internships, **companies** publish internship opportunities, and **admins** manage the
platform. The project exists primarily for **learning and CV-building**: the aim is not
just a working app, but understanding how a real software project moves from requirements
→ database → API → auth → business rules → frontend → tests → docs → Docker → deployment.

## User Roles

| Role | Purpose |
|---|---|
| **Student** | Registers, completes a profile, browses open internships, applies, and tracks/withdraws applications. |
| **Company** | Registers, completes a profile, waits for admin approval, publishes internships, and reviews applicants. |
| **Admin** | Approves/rejects companies, views all users and applications, disables users, and monitors platform statistics. |

## MVP Scope (Version 1 — must include)

- Register / Login with three roles (Student, Company, Admin)
- Student profile and Company profile
- Company approval by admin (companies cannot publish until approved)
- Internship CRUD + publishing workflow (Draft → Open → Closed / Cancelled)
- Public internship listing (only Open internships are visible to students)
- Student application workflow (apply, track, withdraw) with core rules
- Company application review (shortlist / accept / reject its own applicants)
- Basic admin dashboard (platform statistics)
- Swagger/OpenAPI documentation
- README + `/docs` documentation
- Database migrations
- Simple React + TypeScript frontend consuming the API
- Automated tests for core business rules
- Docker Compose setup and a deployment

## Out of Scope (Version 1 — deliberately excluded)

- Payments
- Chat system
- Real-time notifications
- AI recommendations
- Advanced analytics
- Advanced file storage / CV file uploads
- Complex UI animations
- Mobile app

These may be added later, **only after** the main system is complete. See
[Future Improvements](../README.md#future-improvements).

## Success Definition

The project is successful when the backend works, auth/roles work, the full workflow
(company registers → admin approves → posts internship → student applies → company reviews)
runs end-to-end, the frontend consumes the API, the project runs locally and via Docker,
it is deployed or deployment-ready, and **the developer can explain every major part in an
interview.**
