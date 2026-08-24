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
