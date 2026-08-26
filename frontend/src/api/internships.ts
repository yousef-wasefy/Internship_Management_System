import { apiRequest } from './client'
import type {
  Application,
  ApplyRequest,
  CreateInternshipRequest,
  InternshipDetails,
  InternshipListItem,
  InternshipQuery,
  InternshipStatus,
  PagedResult,
  UpdateInternshipRequest,
} from '../types'

export function getInternships(query: InternshipQuery) {
  const params = new URLSearchParams()
  if (query.location) params.set('location', query.location)
  if (query.workMode) params.set('workMode', query.workMode)
  if (query.search) params.set('search', query.search)
  params.set('page', String(query.page ?? 1))
  params.set('pageSize', String(query.pageSize ?? 10))

  return apiRequest<PagedResult<InternshipListItem>>(`/internships?${params.toString()}`)
}

export function getInternshipById(id: number) {
  return apiRequest<InternshipDetails>(`/internships/${id}`)
}

export function applyToInternship(id: number, dto: ApplyRequest, token: string) {
  return apiRequest<Application>(`/internships/${id}/apply`, { method: 'POST', body: dto, token })
}

export function createInternship(dto: CreateInternshipRequest, token: string) {
  return apiRequest<InternshipDetails>('/internships', { method: 'POST', body: dto, token })
}

export function updateInternship(id: number, dto: UpdateInternshipRequest, token: string) {
  return apiRequest<void>(`/internships/${id}`, { method: 'PUT', body: dto, token })
}

export function deleteInternship(id: number, token: string) {
  return apiRequest<void>(`/internships/${id}`, { method: 'DELETE', token })
}

export function openInternship(id: number, token: string) {
  return apiRequest<InternshipDetails>(`/internships/${id}/open`, { method: 'PATCH', token })
}

export function closeInternship(id: number, token: string) {
  return apiRequest<InternshipDetails>(`/internships/${id}/close`, { method: 'PATCH', token })
}

export function getMyInternships(token: string, status?: InternshipStatus) {
  const suffix = status ? `?status=${status}` : ''
  return apiRequest<InternshipListItem[]>(`/companies/me/internships${suffix}`, { token })
}

export function getMyInternshipById(id: number, token: string) {
  return apiRequest<InternshipDetails>(`/companies/me/internships/${id}`, { token })
}
