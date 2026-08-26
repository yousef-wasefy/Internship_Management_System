import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ApiError } from '../../api/client'
import { createInternship, getMyInternshipById, updateInternship } from '../../api/internships'
import { ErrorMessage } from '../../components/ErrorMessage'
import { useAuth } from '../../context/AuthContext'
import type { CreateInternshipRequest, WorkMode } from '../../types'

const WORK_MODES: WorkMode[] = ['Onsite', 'Remote', 'Hybrid']

const emptyForm: CreateInternshipRequest = {
  title: '',
  description: '',
  requirements: '',
  responsibilities: '',
  location: '',
  workMode: 'Onsite',
  duration: '',
  applicationDeadline: '',
}

// The deadline input is a plain <input type="datetime-local">, sent to the API as-is -
// the backend treats a timezone-less datetime as UTC (see InternshipService.AsUtc), so
// this deliberately does not attempt any local-timezone conversion: the value shown in
// the form is exactly the value stored.
function toDateTimeLocalValue(iso: string) {
  return iso.slice(0, 16)
}

export function InternshipFormPage() {
  const { id } = useParams<{ id: string }>()
  const isEdit = id !== undefined
  const { token } = useAuth()
  const navigate = useNavigate()

  const [form, setForm] = useState<CreateInternshipRequest>(emptyForm)
  const [loading, setLoading] = useState(isEdit)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]> | undefined>()
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!isEdit || !token || !id) return
    getMyInternshipById(Number(id), token)
      .then((internship) =>
        setForm({
          title: internship.title,
          description: internship.description ?? '',
          requirements: internship.requirements ?? '',
          responsibilities: internship.responsibilities ?? '',
          location: internship.location ?? '',
          workMode: internship.workMode,
          duration: internship.duration ?? '',
          applicationDeadline: toDateTimeLocalValue(internship.applicationDeadline),
        }),
      )
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Could not load this internship.'))
      .finally(() => setLoading(false))
  }, [isEdit, token, id])

  function updateField<K extends keyof CreateInternshipRequest>(key: K, value: CreateInternshipRequest[K]) {
    setForm((prev) => ({ ...prev, [key]: value }))
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!token) return
    setError(null)
    setFieldErrors(undefined)
    setSubmitting(true)
    try {
      if (isEdit && id) {
        await updateInternship(Number(id), form, token)
      } else {
        await createInternship(form, token)
      }
      navigate('/company/dashboard')
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

  if (loading) return <div className="page">Loading…</div>
  if (loadError) return <div className="page"><ErrorMessage message={loadError} /></div>

  return (
    <div className="page page-narrow">
      <h1>{isEdit ? 'Edit Internship' : 'New Internship'}</h1>
      <form onSubmit={handleSubmit} className="form">
        <label>
          Title
          <input value={form.title} onChange={(e) => updateField('title', e.target.value)} required />
        </label>
        <label>
          Description
          <textarea value={form.description} onChange={(e) => updateField('description', e.target.value)} rows={4} />
        </label>
        <label>
          Requirements
          <textarea value={form.requirements} onChange={(e) => updateField('requirements', e.target.value)} rows={3} />
        </label>
        <label>
          Responsibilities
          <textarea
            value={form.responsibilities}
            onChange={(e) => updateField('responsibilities', e.target.value)}
            rows={3}
          />
        </label>
        <label>
          Location
          <input value={form.location} onChange={(e) => updateField('location', e.target.value)} />
        </label>
        <label>
          Work mode
          <select value={form.workMode} onChange={(e) => updateField('workMode', e.target.value as WorkMode)}>
            {WORK_MODES.map((mode) => (
              <option key={mode} value={mode}>
                {mode}
              </option>
            ))}
          </select>
        </label>
        <label>
          Duration
          <input placeholder="e.g. 3 months" value={form.duration} onChange={(e) => updateField('duration', e.target.value)} />
        </label>
        <label>
          Application deadline
          <input
            type="datetime-local"
            value={form.applicationDeadline}
            onChange={(e) => updateField('applicationDeadline', e.target.value)}
            required
          />
        </label>
        <ErrorMessage message={error} fieldErrors={fieldErrors} />
        <div className="form-actions">
          <button type="submit" disabled={submitting}>
            {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Create'}
          </button>
          <button type="button" className="secondary" onClick={() => navigate('/company/dashboard')}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  )
}
