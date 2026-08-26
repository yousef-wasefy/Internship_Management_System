import type { ReactElement } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import type { UserRole } from '../types'

interface ProtectedRouteProps {
  role?: UserRole
  children: ReactElement
}

// Wraps a route that requires login (and optionally a specific role). Not logged in ->
// bounce to /login, remembering where the user was headed so LoginPage can send them
// back. Logged in as the wrong role -> bounce to the public listing rather than an
// error page, since "wrong role for this page" isn't the same as "something's broken."
export function ProtectedRoute({ role, children }: ProtectedRouteProps) {
  const auth = useAuth()
  const location = useLocation()

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }
  if (role && auth.role !== role) {
    return <Navigate to="/" replace />
  }
  return children
}
