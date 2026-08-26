import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { registerCompany, registerStudent } from '../api/auth'
import { ApiError } from '../api/client'
import { ErrorMessage } from '../components/ErrorMessage'
import { useAuth } from '../context/AuthContext'
import type { UserRole } from '../types'

export function RegisterPage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const [role, setRole] = useState<Extract<UserRole, 'Student' | 'Company'>>('Student')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [name, setName] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]> | undefined>()
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setFieldErrors(undefined)
    setSubmitting(true)
    try {
      const response =
        role === 'Student'
          ? await registerStudent({ email, password, fullName: name })
          : await registerCompany({ email, password, companyName: name })
      auth.login(response)
      navigate('/dashboard')
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
        setFieldErrors(err.errors)
      } else {
        setError('Something went wrong. Please try again.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page page-narrow">
      <h1>Register</h1>
      <div className="role-toggle">
        <button type="button" className={role === 'Student' ? 'active' : ''} onClick={() => setRole('Student')}>
          I'm a Student
        </button>
        <button type="button" className={role === 'Company' ? 'active' : ''} onClick={() => setRole('Company')}>
          I'm a Company
        </button>
      </div>
      {role === 'Company' && (
        <p className="hint">
          Company accounts need admin approval before they can publish internships. You can still register and log
          in right away.
        </p>
      )}
      <form onSubmit={handleSubmit} className="form">
        <label>
          {role === 'Student' ? 'Full name' : 'Company name'}
          <input type="text" value={name} onChange={(e) => setName(e.target.value)} required />
        </label>
        <label>
          Email
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            minLength={8}
            required
          />
        </label>
        <ErrorMessage message={error} fieldErrors={fieldErrors} />
        <button type="submit" disabled={submitting}>
          {submitting ? 'Registering…' : 'Register'}
        </button>
      </form>
    </div>
  )
}
