import { apiRequest } from './client'
import type { Application, ApplyRequest, InternshipDetails, InternshipListItem, InternshipQuery, PagedResult } from '../types'

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
