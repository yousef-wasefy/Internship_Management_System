import { useEffect, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { applyToInternship, getInternshipById } from '../api/internships'
import { ApiError } from '../api/client'
import { ErrorMessage } from '../components/ErrorMessage'
import { useAuth } from '../context/AuthContext'
import type { InternshipDetails } from '../types'

export function InternshipDetailPage() {
  const { id } = useParams<{ id: string }>()
  const auth = useAuth()

  const [internship, setInternship] = useState<InternshipDetails | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  const [coverLetter, setCoverLetter] = useState('')
  const [cvUrl, setCvUrl] = useState('')
  const [applyError, setApplyError] = useState<string | null>(null)
  const [applyFieldErrors, setApplyFieldErrors] = useState<Record<string, string[]> | undefined>()
  const [applied, setApplied] = useState(false)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!id) return
    setLoading(true)
    getInternshipById(Number(id))
      .then(setInternship)
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Could not load this internship.'))
      .finally(() => setLoading(false))
  }, [id])

  async function handleApply(e: FormEvent) {
    e.preventDefault()
    if (!id || !auth.token) return

    setApplyError(null)
    setApplyFieldErrors(undefined)
    setSubmitting(true)
    try {
      await applyToInternship(Number(id), { coverLetter: coverLetter || undefined, cvUrl: cvUrl || undefined }, auth.token)
      setApplied(true)
    } catch (err) {
      if (err instanceof ApiError) {
        setApplyError(err.message)
        setApplyFieldErrors(err.errors)
      } else {
        setApplyError('Something went wrong. Please try again.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <div className="page">Loading…</div>
  if (loadError) return <div className="page"><ErrorMessage message={loadError} /></div>
  if (!internship) return null

  return (
    <div className="page page-narrow">
      <Link to="/">&larr; Back to listings</Link>
      <h1>{internship.title}</h1>
      <p className="internship-meta">
        {internship.companyName} · {internship.location ?? 'Location not specified'} · {internship.workMode}
        {internship.duration ? ` · ${internship.duration}` : ''}
      </p>
      <p className="internship-meta">
        Apply by {new Date(internship.applicationDeadline).toLocaleDateString()}
      </p>

      {internship.description && (
        <section>
          <h2>Description</h2>
          <p>{internship.description}</p>
        </section>
      )}
      {internship.requirements && (
        <section>
          <h2>Requirements</h2>
          <p>{internship.requirements}</p>
        </section>
      )}
      {internship.responsibilities && (
        <section>
          <h2>Responsibilities</h2>
          <p>{internship.responsibilities}</p>
        </section>
      )}

      <section>
        <h2>Apply</h2>
        {applied ? (
          <p className="success-box">
            Application submitted! Track it from <Link to="/student/dashboard">your dashboard</Link>.
          </p>
        ) : !auth.isAuthenticated ? (
          <p className="hint">
            <Link to="/login">Log in</Link> as a Student to apply.
          </p>
        ) : auth.role !== 'Student' ? (
          <p className="hint">Only Student accounts can apply to internships.</p>
        ) : internship.status !== 'Open' ? (
          <p className="hint">This internship is not currently accepting applications.</p>
        ) : (
          <form onSubmit={handleApply} className="form">
            <label>
              Cover letter (optional)
              <textarea value={coverLetter} onChange={(e) => setCoverLetter(e.target.value)} rows={4} />
            </label>
            <label>
              CV link (optional)
              <input
                type="url"
                placeholder="https://…"
                value={cvUrl}
                onChange={(e) => setCvUrl(e.target.value)}
              />
            </label>
            <ErrorMessage message={applyError} fieldErrors={applyFieldErrors} />
            <button type="submit" disabled={submitting}>
              {submitting ? 'Submitting…' : 'Submit application'}
            </button>
          </form>
        )}
      </section>
    </div>
  )
}
