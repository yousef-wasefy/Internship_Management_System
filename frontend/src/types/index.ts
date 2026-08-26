// Mirrors the backend's DTOs and enums exactly (see backend/.../DTOs and
// backend/.../Enums) - kept in this one file since the frontend is small enough that a
// generated client would be more ceremony than it's worth. Program.cs serializes enums
// as their string names (JsonStringEnumConverter), so these are string unions, not
// numbers.

export type UserRole = 'Student' | 'Company' | 'Admin'
export type WorkMode = 'Onsite' | 'Remote' | 'Hybrid'
export type InternshipStatus = 'Draft' | 'Open' | 'Closed' | 'Cancelled'
export type ApplicationStatus = 'Pending' | 'Shortlisted' | 'Accepted' | 'Rejected' | 'Withdrawn'

export interface AuthResponse {
  token: string
  expiresAt: string
  email: string
  role: UserRole
}

export interface CurrentUser {
  id: number
  email: string
  role: UserRole
}

export interface RegisterStudentRequest {
  email: string
  password: string
  fullName: string
}

export interface RegisterCompanyRequest {
  email: string
  password: string
  companyName: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface InternshipListItem {
  id: number
  title: string
  location: string | null
  workMode: WorkMode
  applicationDeadline: string
  status: InternshipStatus
  companyName: string
}

export interface InternshipDetails {
  id: number
  title: string
  description: string | null
  requirements: string | null
  responsibilities: string | null
  location: string | null
  workMode: WorkMode
  duration: string | null
  applicationDeadline: string
  status: InternshipStatus
  companyName: string
  createdAt: string
  updatedAt: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface InternshipQuery {
  location?: string
  workMode?: WorkMode
  search?: string
  page?: number
  pageSize?: number
}

export interface ApplyRequest {
  coverLetter?: string
  cvUrl?: string
}

export interface Application {
  id: number
  internshipPostId: number
  internshipTitle: string
  companyName: string
  coverLetter: string | null
  cvUrl: string | null
  status: ApplicationStatus
  appliedAt: string
  updatedAt: string
  reviewedAt: string | null
  companyNotes: string | null
}

export interface StudentProfile {
  id: number
  email: string
  fullName: string
  university: string | null
  faculty: string | null
  major: string | null
  academicYear: string | null
  skills: string | null
  cvUrl: string | null
  linkedInUrl: string | null
  gitHubUrl: string | null
  createdAt: string
  updatedAt: string
}

export interface UpdateStudentProfileRequest {
  fullName: string
  university?: string
  faculty?: string
  major?: string
  academicYear?: string
  skills?: string
  cvUrl?: string
  linkedInUrl?: string
  gitHubUrl?: string
}

export interface CompanyProfile {
  id: number
  email: string
  companyName: string
  industry: string | null
  websiteUrl: string | null
  description: string | null
  location: string | null
  isApproved: boolean
  createdAt: string
  updatedAt: string
}

export interface UpdateCompanyProfileRequest {
  companyName: string
  industry?: string
  websiteUrl?: string
  description?: string
  location?: string
}

export interface CreateInternshipRequest {
  title: string
  description?: string
  requirements?: string
  responsibilities?: string
  location?: string
  workMode: WorkMode
  duration?: string
  applicationDeadline: string
}

export type UpdateInternshipRequest = CreateInternshipRequest

export interface Applicant {
  id: number
  internshipPostId: number
  internshipTitle: string
  studentFullName: string
  studentEmail: string
  studentUniversity: string | null
  studentMajor: string | null
  studentSkills: string | null
  studentLinkedInUrl: string | null
  studentGitHubUrl: string | null
  coverLetter: string | null
  cvUrl: string | null
  status: ApplicationStatus
  appliedAt: string
  updatedAt: string
  reviewedAt: string | null
  companyNotes: string | null
}

// The three statuses a company may set via PATCH /api/applications/{id}/status - not
// Pending (the default) or Withdrawn (a student-only action). Enforced server-side in
// ApplicationService.UpdateStatusAsync; mirrored here so the UI only ever offers valid actions.
export type CompanySettableStatus = 'Shortlisted' | 'Accepted' | 'Rejected'

export interface UpdateApplicationStatusRequest {
  status: CompanySettableStatus
  companyNotes?: string
}

export interface AdminDashboard {
  totalStudents: number
  totalCompanies: number
  pendingCompanies: number
  totalInternships: number
  openInternships: number
  totalApplications: number
  acceptedApplications: number
  rejectedApplications: number
}

export interface AdminUser {
  id: number
  email: string
  displayName: string | null
  role: UserRole
  isDisabled: boolean
  createdAt: string
}
