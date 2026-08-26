import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { ApiError } from '../../api/client'
import { getMyCompanyProfile, updateMyCompanyProfile } from '../../api/companies'
import { closeInternship, deleteInternship, getMyInternships, openInternship } from '../../api/internships'
import { ErrorMessage } from '../../components/ErrorMessage'
import { useAuth } from '../../context/AuthContext'
import type { CompanyProfile, InternshipListItem, InternshipStatus, UpdateCompanyProfileRequest } from '../../types'

const STATUS_TABS: Array<InternshipStatus | 'All'> = ['All', 'Draft', 'Open', 'Closed', 'Cancelled']

export function CompanyDashboardPage() {
  const { token } = useAuth()
  const [profile, setProfile] = useState<CompanyProfile | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [editing, setEditing] = useState(false)
  const [form, setForm] = useState<UpdateCompanyProfileRequest>({ companyName: '' })
  const [saveError, setSaveError] = useState<string | null>(null)
  const [saveFieldErrors, setSaveFieldErrors] = useState<Record<string, string[]> | undefined>()
  const [saving, setSaving] = useState(false)

  const [statusFilter, setStatusFilter] = useState<InternshipStatus | 'All'>('All')
  const [internships, setInternships] = useState<InternshipListItem[]>([])
  const [listError, setListError] = useState<string | null>(null)
  const [listLoading, setListLoading] = useState(true)

  useEffect(() => {
    if (!token) return
    getMyCompanyProfile(token)
      .then((result) => {
        setProfile(result)
        setForm({
          companyName: result.companyName,
          industry: result.industry ?? '',
          websiteUrl: result.websiteUrl ?? '',
          description: result.description ?? '',
          location: result.location ?? '',
        })
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Could not load your dashboard.'))
      .finally(() => setLoading(false))
  }, [token])

  function loadInternships() {
    if (!token) return
    setListLoading(true)
    setListError(null)
    getMyInternships(token, statusFilter === 'All' ? undefined : statusFilter)
      .then(setInternships)
      .catch((err) => setListError(err instanceof ApiError ? err.message : 'Could not load your internships.'))
      .finally(() => setListLoading(false))
  }

  useEffect(loadInternships, [token, statusFilter])

  function updateField<K extends keyof UpdateCompanyProfileRequest>(key: K, value: UpdateCompanyProfileRequest[K]) {
    setForm((prev) => ({ ...prev, [key]: value }))
  }

  async function handleSaveProfile(e: FormEvent) {
    e.preventDefault()
    if (!token) return
    setSaveError(null)
    setSaveFieldErrors(undefined)
    setSaving(true)
    try {
      await updateMyCompanyProfile(form, token)
      const refreshed = await getMyCompanyProfile(token)
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

  async function handleOpen(id: number) {
    if (!token) return
    setListError(null)
    try {
      await openInternship(id, token)
      loadInternships()
    } catch (err) {
      setListError(err instanceof ApiError ? err.message : 'Could not open this internship.')
    }
  }

  async function handleClose(id: number) {
    if (!token) return
    setListError(null)
    try {
      await closeInternship(id, token)
      loadInternships()
    } catch (err) {
      setListError(err instanceof ApiError ? err.message : 'Could not close this internship.')
    }
  }

  async function handleDelete(id: number) {
    if (!token) return
    if (!window.confirm('Delete this internship post permanently? This cannot be undone.')) return
    setListError(null)
    try {
      await deleteInternship(id, token)
      loadInternships()
    } catch (err) {
      setListError(err instanceof ApiError ? err.message : 'Could not delete this internship.')
    }
  }

  if (loading) return <div className="page">Loading…</div>
  if (loadError) return <div className="page"><ErrorMessage message={loadError} /></div>
  if (!profile) return null

  return (
    <div className="page">
      <h1>Company Dashboard</h1>

      <section className="dashboard-section">
        <div className="section-header">
          <h2>Profile</h2>
          {!editing && (
            <button type="button" onClick={() => setEditing(true)}>
              Edit profile
            </button>
          )}
        </div>

        <p>
          Approval status:{' '}
          <span className={`badge ${profile.isApproved ? 'badge-approved' : 'badge-pending'}`}>
            {profile.isApproved ? 'Approved' : 'Pending admin approval'}
          </span>
        </p>
        {!profile.isApproved && (
          <p className="hint">You can create Draft internships now, but an admin must approve your company before you can open one.</p>
        )}

        {editing ? (
          <form onSubmit={handleSaveProfile} className="form">
            <label>
              Company name
              <input value={form.companyName} onChange={(e) => updateField('companyName', e.target.value)} required />
            </label>
            <label>
              Industry
              <input value={form.industry} onChange={(e) => updateField('industry', e.target.value)} />
            </label>
            <label>
              Website
              <input type="url" value={form.websiteUrl} onChange={(e) => updateField('websiteUrl', e.target.value)} />
            </label>
            <label>
              Location
              <input value={form.location} onChange={(e) => updateField('location', e.target.value)} />
            </label>
            <label>
              Description
              <textarea value={form.description} onChange={(e) => updateField('description', e.target.value)} rows={4} />
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
            <dt>Company name</dt>
            <dd>{profile.companyName}</dd>
            <dt>Industry</dt>
            <dd>{profile.industry || '—'}</dd>
            <dt>Website</dt>
            <dd>{profile.websiteUrl ? <a href={profile.websiteUrl} target="_blank" rel="noreferrer">{profile.websiteUrl}</a> : '—'}</dd>
            <dt>Location</dt>
            <dd>{profile.location || '—'}</dd>
            <dt>Description</dt>
            <dd>{profile.description || '—'}</dd>
          </dl>
        )}
      </section>

      <section className="dashboard-section">
        <div className="section-header">
          <h2>My Internships</h2>
          <Link to="/company/internships/new">
            <button type="button">+ New Internship</button>
          </Link>
        </div>

        <div className="role-toggle">
          {STATUS_TABS.map((tab) => (
            <button
              key={tab}
              type="button"
              className={statusFilter === tab ? 'active' : ''}
              onClick={() => setStatusFilter(tab)}
            >
              {tab}
            </button>
          ))}
        </div>

        <ErrorMessage message={listError} />

        {listLoading ? (
          <p>Loading…</p>
        ) : internships.length === 0 ? (
          <p className="hint">No internships in this category yet.</p>
        ) : (
          <ul className="card-list">
            {internships.map((post) => (
              <li key={post.id} className="dashboard-card">
                <div className="card-header">
                  <h3>{post.title}</h3>
                  <span className={`status-badge status-${post.status.toLowerCase()}`}>{post.status}</span>
                </div>
                <p className="internship-meta">
                  {post.location ?? 'Location not specified'} · {post.workMode} · Deadline{' '}
                  {new Date(post.applicationDeadline).toLocaleDateString()}
                </p>
                <div className="card-actions">
                  <Link to={`/company/internships/${post.id}/edit`}>
                    <button type="button" className="secondary">Edit</button>
                  </Link>
                  <Link to={`/company/internships/${post.id}/applicants`}>
                    <button type="button" className="secondary">View Applicants</button>
                  </Link>
                  {(post.status === 'Draft' || post.status === 'Closed') && (
                    <button type="button" onClick={() => handleOpen(post.id)}>
                      Open
                    </button>
                  )}
                  {post.status === 'Open' && (
                    <button type="button" onClick={() => handleClose(post.id)}>
                      Close
                    </button>
                  )}
                  <button type="button" className="danger" onClick={() => handleDelete(post.id)}>
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
