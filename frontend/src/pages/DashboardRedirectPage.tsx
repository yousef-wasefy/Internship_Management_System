import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

// /dashboard itself renders nothing - it just sends each role to its own dashboard, so
// the rest of the app (Navbar, LoginPage's post-login redirect) can link to one place
// without needing to know the current role.
export function DashboardRedirectPage() {
  const { role } = useAuth()

  switch (role) {
    case 'Student':
      return <Navigate to="/student/dashboard" replace />
    case 'Company':
      return <Navigate to="/company/dashboard" replace />
    case 'Admin':
      return <Navigate to="/admin/dashboard" replace />
    default:
      return <Navigate to="/" replace />
  }
}
