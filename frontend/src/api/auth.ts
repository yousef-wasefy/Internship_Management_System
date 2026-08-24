import { apiRequest } from './client'
import type { AuthResponse, CurrentUser, LoginRequest, RegisterCompanyRequest, RegisterStudentRequest } from '../types'

export function registerStudent(dto: RegisterStudentRequest) {
  return apiRequest<AuthResponse>('/auth/register-student', { method: 'POST', body: dto })
}

export function registerCompany(dto: RegisterCompanyRequest) {
  return apiRequest<AuthResponse>('/auth/register-company', { method: 'POST', body: dto })
}

export function login(dto: LoginRequest) {
  return apiRequest<AuthResponse>('/auth/login', { method: 'POST', body: dto })
}

export function getCurrentUser(token: string) {
  return apiRequest<CurrentUser>('/auth/me', { token })
}
