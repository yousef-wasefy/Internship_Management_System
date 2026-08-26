import { apiRequest } from './client'
import type { Applicant, Application, UpdateApplicationStatusRequest } from '../types'

export function getMyApplications(token: string) {
  return apiRequest<Application[]>('/applications/my', { token })
}

export function withdrawApplication(id: number, token: string) {
  return apiRequest<Application>(`/applications/${id}/withdraw`, { method: 'PATCH', token })
}

export function getApplicantsForInternship(internshipId: number, token: string) {
  return apiRequest<Applicant[]>(`/internships/${internshipId}/applications`, { token })
}

export function updateApplicationStatus(id: number, dto: UpdateApplicationStatusRequest, token: string) {
  return apiRequest<Applicant>(`/applications/${id}/status`, { method: 'PATCH', body: dto, token })
}
