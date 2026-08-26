import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export function Navbar() {
  const { isAuthenticated, email, role, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <header className="navbar">
      <Link to="/" className="navbar-brand">
        Internship Management System
      </Link>
      <nav className="navbar-links">
        {isAuthenticated ? (
          <>
            <Link to="/">Browse Internships</Link>
            <Link to="/dashboard">Dashboard</Link>
            <span className="navbar-user">
              {email} <span className="badge">{role}</span>
            </span>
            <button type="button" className="link-button" onClick={handleLogout}>
              Log out
            </button>
          </>
        ) : (
          <>
            <Link to="/login">Log in</Link>
            <Link to="/register">Register</Link>
          </>
        )}
      </nav>
    </header>
  )
}
