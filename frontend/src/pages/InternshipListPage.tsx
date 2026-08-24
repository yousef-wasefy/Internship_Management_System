import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getInternships } from '../api/internships'
import { ApiError } from '../api/client'
import { ErrorMessage } from '../components/ErrorMessage'
import type { InternshipListItem, WorkMode } from '../types'

const WORK_MODES: WorkMode[] = ['Onsite', 'Remote', 'Hybrid']
const PAGE_SIZE = 10

export function InternshipListPage() {
  const [items, setItems] = useState<InternshipListItem[]>([])
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage] = useState(1)
  const [location, setLocation] = useState('')
  const [workMode, setWorkMode] = useState<WorkMode | ''>('')
  const [search, setSearch] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    getInternships({
      location: location || undefined,
      workMode: workMode || undefined,
      search: search || undefined,
      page,
      pageSize: PAGE_SIZE,
    })
      .then((result) => {
        if (cancelled) return
        setItems(result.items)
        setTotalPages(result.totalPages)
      })
      .catch((err) => {
        if (cancelled) return
        setError(err instanceof ApiError ? err.message : 'Could not load internships.')
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [location, workMode, search, page])

  function handleFilterChange<T>(setter: (value: T) => void, value: T) {
    setter(value)
    setPage(1)
  }

  return (
    <div className="page">
      <h1>Open Internships</h1>

      <form className="filters" onSubmit={(e) => e.preventDefault()}>
        <input
          type="text"
          placeholder="Search by title…"
          value={search}
          onChange={(e) => handleFilterChange(setSearch, e.target.value)}
        />
        <input
          type="text"
          placeholder="Location…"
          value={location}
          onChange={(e) => handleFilterChange(setLocation, e.target.value)}
        />
        <select value={workMode} onChange={(e) => handleFilterChange(setWorkMode, e.target.value as WorkMode | '')}>
          <option value="">Any work mode</option>
          {WORK_MODES.map((mode) => (
            <option key={mode} value={mode}>
              {mode}
            </option>
          ))}
        </select>
      </form>

      <ErrorMessage message={error} />

      {loading ? (
        <p>Loading…</p>
      ) : items.length === 0 ? (
        <p>No internships match these filters.</p>
      ) : (
        <ul className="internship-list">
          {items.map((item) => (
            <li key={item.id} className="internship-card">
              <Link to={`/internships/${item.id}`}>
                <h2>{item.title}</h2>
              </Link>
              <p className="internship-meta">
                {item.companyName} · {item.location ?? 'Location not specified'} · {item.workMode}
              </p>
              <p className="internship-meta">
                Apply by {new Date(item.applicationDeadline).toLocaleDateString()}
              </p>
            </li>
          ))}
        </ul>
      )}

      {totalPages > 1 && (
        <div className="pagination">
          <button type="button" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            Previous
          </button>
          <span>
            Page {page} of {totalPages}
          </span>
          <button type="button" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
            Next
          </button>
        </div>
      )}
    </div>
  )
}
