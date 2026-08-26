import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { getMyApplications, withdrawApplication } from '../../api/applications'
import { ApiError } from '../../api/client'
import { getMyStudentProfile, updateMyStudentProfile } from '../../api/students'
import { ErrorMessage } from '../../components/ErrorMessage'
import { useAuth } from '../../context/AuthContext'
import type { Application, StudentProfile, UpdateStudentProfileRequest } from '../../types'

const emptyForm: UpdateStudentProfileRequest = {
  fullName: '',
  university: '',
  faculty: '',
  major: '',
  academicYear: '',
  skills: '',
  cvUrl: '',
  linkedInUrl: '',
  gitHubUrl: '',
}

export function StudentDashboardPage() {
  const { token } = useAuth()
  const [profile, setProfile] = useState<StudentProfile | null>(null)
  const [applications, setApplications] = useState<Application[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [editing, setEditing] = useState(false)
  const [form, setForm] = useState<UpdateStudentProfileRequest>(emptyForm)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [saveFieldErrors, setSaveFieldErrors] = useState<Record<string, string[]> | undefined>()
  const [saving, setSaving] = useState(false)

  const [actionError, setActionError] = useState<string | null>(null)

  useEffect(() => {
    if (!token) return
    Promise.all([getMyStudentProfile(token), getMyApplications(token)])
      .then(([profileResult, applicationsResult]) => {
        setProfile(profileResult)
        setApplications(applicationsResult)
        setForm({
          fullName: profileResult.fullName,
          university: profileResult.university ?? '',
          faculty: profileResult.faculty ?? '',
          major: profileResult.major ?? '',
          academicYear: profileResult.academicYear ?? '',
          skills: profileResult.skills ?? '',
          cvUrl: profileResult.cvUrl ?? '',
          linkedInUrl: profileResult.linkedInUrl ?? '',
          gitHubUrl: profileResult.gitHubUrl ?? '',
        })
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Could not load your dashboard.'))
      .finally(() => setLoading(false))
  }, [token])

  function updateField<K extends keyof UpdateStudentProfileRequest>(key: K, value: UpdateStudentProfileRequest[K]) {
    setForm((prev) => ({ ...prev, [key]: value }))
  }

  async function handleSaveProfile(e: FormEvent) {
    e.preventDefault()
    if (!token) return
    setSaveError(null)
    setSaveFieldErrors(undefined)
    setSaving(true)
    try {
      await updateMyStudentProfile(form, token)
      const refreshed = await getMyStudentProfile(token)
      setProfile(refreshed)
      setEditing(false)
    } catch (err) {
      if (err instanceof ApiError) {
        setSaveError(err.message)
        setSaveFieldErrors(err.errors)
      } else {
        setSaveError('Something went wrong. Please try again.')
      }
    } finally {
      setSaving(false)
    }
  }

  async function handleWithdraw(applicationId: number) {
    if (!token) return
    setActionError(null)
    try {
      const updated = await withdrawApplication(applicationId, token)
      setApplications((prev) => prev.map((app) => (app.id === applicationId ? updated : app)))
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : 'Could not withdraw this application.')
    }
  }

  if (loading) return <div className="page">Loading…</div>
  if (loadError) return <div className="page"><ErrorMessage message={loadError} /></div>
  if (!profile) return null

  return (
    <div className="page">
      <h1>My Dashboard</h1>

      <section className="dashboard-section">
        <div className="section-header">
          <h2>Profile</h2>
          {!editing && (
            <button type="button" onClick={() => setEditing(true)}>
              Edit profile
            </button>
          )}
        </div>

        {editing ? (
          <form onSubmit={handleSaveProfile} className="form">
            <label>
              Full name
              <input value={form.fullName} onChange={(e) => updateField('fullName', e.target.value)} required />
            </label>
            <label>
              University
              <input value={form.university} onChange={(e) => updateField('university', e.target.value)} />
            </label>
            <label>
              Faculty
              <input value={form.faculty} onChange={(e) => updateField('faculty', e.target.value)} />
            </label>
            <label>
              Major
              <input value={form.major} onChange={(e) => updateField('major', e.target.value)} />
            </label>
            <label>
              Academic year
              <input value={form.academicYear} onChange={(e) => updateField('academicYear', e.target.value)} />
            </label>
            <label>
              Skills
              <input value={form.skills} onChange={(e) => updateField('skills', e.target.value)} />
            </label>
            <label>
              CV link
              <input type="url" value={form.cvUrl} onChange={(e) => updateField('cvUrl', e.target.value)} />
            </label>
            <label>
              LinkedIn
              <input type="url" value={form.linkedInUrl} onChange={(e) => updateField('linkedInUrl', e.target.value)} />
            </label>
            <label>
              GitHub
              <input type="url" value={form.gitHubUrl} onChange={(e) => updateField('gitHubUrl', e.target.value)} />
            </label>
            <ErrorMessage message={saveError} fieldErrors={saveFieldErrors} />
            <div className="form-actions">
              <button type="submit" disabled={saving}>
                {saving ? 'Saving…' : 'Save'}
              </button>
              <button type="button" className="secondary" onClick={() => setEditing(false)}>
                Cancel
              </button>
            </div>
          </form>
        ) : (
          <dl className="profile-view">
            <dt>Full name</dt>
            <dd>{profile.fullName}</dd>
            <dt>University</dt>
            <dd>{profile.university || '—'}</dd>
            <dt>Faculty</dt>
            <dd>{profile.faculty || '—'}</dd>
            <dt>Major</dt>
            <dd>{profile.major || '—'}</dd>
            <dt>Academic year</dt>
            <dd>{profile.academicYear || '—'}</dd>
            <dt>Skills</dt>
            <dd>{profile.skills || '—'}</dd>
            <dt>CV</dt>
            <dd>{profile.cvUrl ? <a href={profile.cvUrl} target="_blank" rel="noreferrer">{profile.cvUrl}</a> : '—'}</dd>
            <dt>LinkedIn</dt>
            <dd>{profile.linkedInUrl ? <a href={profile.linkedInUrl} target="_blank" rel="noreferrer">{profile.linkedInUrl}</a> : '—'}</dd>
            <dt>GitHub</dt>
            <dd>{profile.gitHubUrl ? <a href={profile.gitHubUrl} target="_blank" rel="noreferrer">{profile.gitHubUrl}</a> : '—'}</dd>
          </dl>
        )}
      </section>

      <section className="dashboard-section">
        <h2>My Applications</h2>
        <ErrorMessage message={actionError} />
        {applications.length === 0 ? (
          <p className="hint">
            You haven't applied to anything yet. <Link to="/">Browse open internships</Link>.
          </p>
        ) : (
          <ul className="card-list">
            {applications.map((app) => (
              <li key={app.id} className="dashboard-card">
                <div className="card-header">
                  <h3>{app.internshipTitle}</h3>
                  <span className={`status-badge status-${app.status.toLowerCase()}`}>{app.status}</span>
                </div>
                <p className="internship-meta">{app.companyName}</p>
                <p className="internship-meta">Applied {new Date(app.appliedAt).toLocaleDateString()}</p>
                {app.companyNotes && <p className="internship-meta">Note from company: {app.companyNotes}</p>}
                {app.status === 'Pending' && (
                  <button type="button" className="secondary" onClick={() => handleWithdraw(app.id)}>
                    Withdraw
                  </button>
                )}
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
