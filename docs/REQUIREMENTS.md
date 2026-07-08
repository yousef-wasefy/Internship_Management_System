# Functional Requirements

This document lists what each role can do and the business rules the system must enforce.
Requirements are grouped by role. Each has an ID (e.g., `STU-1`) so tests and API docs can
reference them.

## 1. Student Requirements

| ID | Requirement |
|---|---|
| STU-1 | A student can register and log in. |
| STU-2 | A student can view and update their own profile (university, faculty, major, academic year, skills, CV/LinkedIn/GitHub links). |
| STU-3 | A student can browse the list of **open** internships. |
| STU-4 | A student can filter/search internships (by location, work mode, title). |
| STU-5 | A student can view the details of a single internship. |
| STU-6 | A student can apply to an open internship. |
| STU-7 | A student can view their own applications and each application's status. |
| STU-8 | A student can withdraw an application **only while it is still Pending**. |

## 2. Company Requirements

| ID | Requirement |
|---|---|
| CO-1 | A company can register and log in. |
| CO-2 | A company can view and update its own profile (name, industry, website, description, location). |
| CO-3 | A company starts **unapproved** and must be approved by an admin before publishing. |
| CO-4 | A company can create internship posts (they start as **Draft**). |
| CO-5 | A company can edit **only its own** internship posts. |
| CO-6 | A company can open/close its own internship posts. |
| CO-7 | A company can view applicants **only for its own** internships. |
| CO-8 | A company can shortlist / accept / reject applicants for its own internships. |
| CO-9 | A company can add optional review notes to an application. |

## 3. Admin Requirements

| ID | Requirement |
|---|---|
| AD-1 | An admin can log in. |
| AD-2 | An admin can view all students and all companies. |
| AD-3 | An admin can approve or reject company accounts. |
| AD-4 | An admin can view all internship posts and all applications. |
| AD-5 | An admin can disable a user. |
| AD-6 | An admin can view system statistics (dashboard). |
| AD-7 | An admin does **not** apply to internships. |

## 4. Core Business Rules

These are the rules that make the system realistic (not just CRUD). They are enforced in
the **service layer** and covered by tests in Phase 15.

### 4.1 Student application rules
1. A student can apply only if logged in.
2. A student can apply only to **Open** internships.
3. A student cannot apply **after the application deadline**.
4. A student **cannot apply twice** to the same internship (enforced by a unique
   constraint on `StudentId + InternshipPostId`).
5. A student can withdraw only **their own** application.
6. A student can withdraw only if the application is still **Pending**.

### 4.2 Company rules
1. A company must be **approved** by an admin before publishing internships.
2. A company can edit only **its own** internship posts.
3. A company can view applicants only for **its own** internships.
4. A company can update application statuses only for **its own** internships.
5. A company cannot review a **Withdrawn** application.

### 4.3 Admin rules
1. Admin can approve or reject company accounts.
2. Admin can view all users and applications.
3. Admin can disable users.
4. Admin can monitor system statistics.
5. Admin does not directly apply to internships.

## 5. Status Workflows

**Internship status:** `Draft → Open → Closed` (and `Cancelled` from Draft/Open).
Only **Open** internships appear in the public listing and accept applications.

**Application status:** `Pending → Shortlisted → Accepted | Rejected`, or
`Pending → Withdrawn` (student action). Once `Withdrawn`, the company cannot change it.
