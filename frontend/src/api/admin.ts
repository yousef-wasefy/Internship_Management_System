import { apiRequest } from './client'
import type { AdminDashboard, AdminUser, CompanyProfile } from '../types'

export function getAdminDashboard(token: string) {
  return apiRequest<AdminDashboard>('/admin/dashboard', { token })
}

export function getPendingCompanies(token: string) {
  return apiRequest<CompanyProfile[]>('/admin/companies/pending', { token })
}

export function approveCompany(id: number, token: string) {
  return apiRequest<CompanyProfile>(`/admin/companies/${id}/approve`, { method: 'PATCH', token })
}

export function rejectCompany(id: number, token: string) {
  return apiRequest<CompanyProfile>(`/admin/companies/${id}/reject`, { method: 'PATCH', token })
}

export function getUsers(token: string) {
  return apiRequest<AdminUser[]>('/admin/users', { token })
}

export function disableUser(id: number, token: string) {
  return apiRequest<AdminUser>(`/admin/users/${id}/disable`, { method: 'PATCH', token })
}
