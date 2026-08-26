import { useEffect, useState } from 'react'
import { approveCompany, getAdminDashboard, getPendingCompanies, getUsers, rejectCompany, disableUser } from '../../api/admin'
import { ApiError } from '../../api/client'
import { ErrorMessage } from '../../components/ErrorMessage'
import { useAuth } from '../../context/AuthContext'
import type { AdminDashboard, AdminUser, CompanyProfile } from '../../types'

export function AdminDashboardPage() {
  const { token } = useAuth()
  const [stats, setStats] = useState<AdminDashboard | null>(null)
  const [pendingCompanies, setPendingCompanies] = useState<CompanyProfile[]>([])
  const [users, setUsers] = useState<AdminUser[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

  function loadAll() {
    if (!token) return
    setLoading(true)
    Promise.all([getAdminDashboard(token), getPendingCompanies(token), getUsers(token)])
      .then(([statsResult, pendingResult, usersResult]) => {
        setStats(statsResult)
        setPendingCompanies(pendingResult)
        setUsers(usersResult)
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Could not load the admin dashboard.'))
      .finally(() => setLoading(false))
  }

  useEffect(loadAll, [token])

  async function handleApprove(id: number) {
    if (!token) return
    setActionError(null)
    try {
      await approveCompany(id, token)
      loadAll()
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : 'Could not approve this company.')
    }
  }

  async function handleReject(id: number) {
    if (!token) return
    if (!window.confirm('Reject this company? This also disables its account.')) return
    setActionError(null)
    try {
      await rejectCompany(id, token)
      loadAll()
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : 'Could not reject this company.')
    }
  }

  async function handleDisable(id: number) {
    if (!token) return
    if (!window.confirm('Disable this user? They will not be able to log in again.')) return
    setActionError(null)
    try {
      await disableUser(id, token)
      loadAll()
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : 'Could not disable this user.')
    }
  }

  if (loading) return <div className="page">Loading…</div>
  if (loadError) return <div className="page"><ErrorMessage message={loadError} /></div>
  if (!stats) return null

  return (
    <div className="page">
      <h1>Admin Dashboard</h1>

      <section className="dashboard-section">
        <h2>Platform Statistics</h2>
        <div className="stat-grid">
          <div className="stat-tile"><span className="stat-value">{stats.totalStudents}</span>Students</div>
          <div className="stat-tile"><span className="stat-value">{stats.totalCompanies}</span>Companies</div>
          <div className="stat-tile"><span className="stat-value">{stats.pendingCompanies}</span>Pending Companies</div>
          <div className="stat-tile"><span className="stat-value">{stats.totalInternships}</span>Internships</div>
          <div className="stat-tile"><span className="stat-value">{stats.openInternships}</span>Open Internships</div>
          <div className="stat-tile"><span className="stat-value">{stats.totalApplications}</span>Applications</div>
          <div className="stat-tile"><span className="stat-value">{stats.acceptedApplications}</span>Accepted</div>
          <div className="stat-tile"><span className="stat-value">{stats.rejectedApplications}</span>Rejected</div>
        </div>
      </section>

      <ErrorMessage message={actionError} />

      <section className="dashboard-section">
        <h2>Pending Company Approvals</h2>
        {pendingCompanies.length === 0 ? (
          <p className="hint">No companies awaiting approval.</p>
        ) : (
          <ul className="card-list">
            {pendingCompanies.map((company) => (
              <li key={company.id} className="dashboard-card">
                <div className="card-header">
                  <h3>{company.companyName}</h3>
                </div>
                <p className="internship-meta">{company.email}</p>
                <p className="internship-meta">{company.industry ?? 'Industry not specified'} · {company.location ?? 'Location not specified'}</p>
                <div className="card-actions">
                  <button type="button" onClick={() => handleApprove(company.id)}>Approve</button>
                  <button type="button" className="danger" onClick={() => handleReject(company.id)}>Reject</button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="dashboard-section">
        <h2>All Users</h2>
        <div className="table-scroll">
          <table>
            <thead>
              <tr>
                <th>Email</th>
                <th>Name</th>
                <th>Role</th>
                <th>Status</th>
                <th>Joined</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.email}</td>
                  <td>{user.displayName ?? '—'}</td>
                  <td>{user.role}</td>
                  <td>
                    <span className={`badge ${user.isDisabled ? 'badge-pending' : 'badge-approved'}`}>
                      {user.isDisabled ? 'Disabled' : 'Active'}
                    </span>
                  </td>
                  <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                  <td>
                    {!user.isDisabled && (
                      <button type="button" className="danger" onClick={() => handleDisable(user.id)}>
                        Disable
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
