import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getApplicantsForInternship, updateApplicationStatus } from '../../api/applications'
import { ApiError } from '../../api/client'
import { ErrorMessage } from '../../components/ErrorMessage'
import { useAuth } from '../../context/AuthContext'
import type { Applicant, CompanySettableStatus } from '../../types'

const ACTIONS: CompanySettableStatus[] = ['Shortlisted', 'Accepted', 'Rejected']

export function ApplicantsPage() {
  const { id } = useParams<{ id: string }>()
  const { token } = useAuth()

  const [applicants, setApplicants] = useState<Applicant[]>([])
  const [notes, setNotes] = useState<Record<number, string>>({})
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [savingId, setSavingId] = useState<number | null>(null)

  useEffect(() => {
    if (!id || !token) return
    getApplicantsForInternship(Number(id), token)
      .then(setApplicants)
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Could not load applicants.'))
      .finally(() => setLoading(false))
  }, [id, token])

  async function handleAction(applicationId: number, status: CompanySettableStatus) {
    if (!token) return
    setActionError(null)
    setSavingId(applicationId)
    try {
      const updated = await updateApplicationStatus(
        applicationId,
        { status, companyNotes: notes[applicationId] || undefined },
        token,
      )
      setApplicants((prev) => prev.map((a) => (a.id === applicationId ? updated : a)))
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : 'Could not update this application.')
    } finally {
      setSavingId(null)
    }
  }

  if (loading) return <div className="page">Loading…</div>
  if (loadError) return <div className="page"><ErrorMessage message={loadError} /></div>

  const title = applicants[0]?.internshipTitle

  return (
    <div className="page">
      <Link to="/company/dashboard">&larr; Back to dashboard</Link>
      <h1>Applicants{title ? ` — ${title}` : ''}</h1>

      <ErrorMessage message={actionError} />

      {applicants.length === 0 ? (
        <p className="hint">No one has applied to this internship yet.</p>
      ) : (
        <ul className="card-list">
          {applicants.map((applicant) => (
            <li key={applicant.id} className="dashboard-card">
              <div className="card-header">
                <h3>{applicant.studentFullName}</h3>
                <span className={`status-badge status-${applicant.status.toLowerCase()}`}>{applicant.status}</span>
              </div>
              <p className="internship-meta">{applicant.studentEmail}</p>
              <p className="internship-meta">
                {applicant.studentUniversity ?? 'University not specified'}
                {applicant.studentMajor ? ` · ${applicant.studentMajor}` : ''}
              </p>
              {applicant.studentSkills && <p className="internship-meta">Skills: {applicant.studentSkills}</p>}
              <p className="internship-meta">
                {applicant.studentLinkedInUrl && (
                  <a href={applicant.studentLinkedInUrl} target="_blank" rel="noreferrer">LinkedIn</a>
                )}
                {applicant.studentLinkedInUrl && applicant.studentGitHubUrl ? ' · ' : ''}
                {applicant.studentGitHubUrl && (
                  <a href={applicant.studentGitHubUrl} target="_blank" rel="noreferrer">GitHub</a>
                )}
              </p>
              {applicant.coverLetter && <p>{applicant.coverLetter}</p>}
              {applicant.cvUrl && (
                <p>
                  <a href={applicant.cvUrl} target="_blank" rel="noreferrer">View CV</a>
                </p>
              )}
              {applicant.companyNotes && <p className="internship-meta">Your note: {applicant.companyNotes}</p>}

              {applicant.status === 'Withdrawn' ? (
                <p className="hint">This application was withdrawn by the student - no action available.</p>
              ) : (
                <>
                  <label>
                    Note (optional, included with your next action)
                    <textarea
                      rows={2}
                      value={notes[applicant.id] ?? ''}
                      onChange={(e) => setNotes((prev) => ({ ...prev, [applicant.id]: e.target.value }))}
                    />
                  </label>
                  <div className="card-actions">
                    {ACTIONS.map((action) => (
                      <button
                        key={action}
                        type="button"
                        className={action === 'Rejected' ? 'danger' : 'secondary'}
                        disabled={savingId === applicant.id}
                        onClick={() => handleAction(applicant.id, action)}
                      >
                        {action}
                      </button>
                    ))}
                  </div>
                </>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
