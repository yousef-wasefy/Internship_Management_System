import { apiRequest } from './client'
import type { StudentProfile, UpdateStudentProfileRequest } from '../types'

export function getMyStudentProfile(token: string) {
  return apiRequest<StudentProfile>('/students/me', { token })
}

export function updateMyStudentProfile(dto: UpdateStudentProfileRequest, token: string) {
  return apiRequest<void>('/students/me', { method: 'PUT', body: dto, token })
}
