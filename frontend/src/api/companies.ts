import { apiRequest } from './client'
import type { CompanyProfile, UpdateCompanyProfileRequest } from '../types'

export function getMyCompanyProfile(token: string) {
  return apiRequest<CompanyProfile>('/companies/me', { token })
}

export function updateMyCompanyProfile(dto: UpdateCompanyProfileRequest, token: string) {
  return apiRequest<void>('/companies/me', { method: 'PUT', body: dto, token })
}
