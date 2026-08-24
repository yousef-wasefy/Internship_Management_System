// Thin fetch wrapper, not a generated client or axios - the API surface this frontend
// talks to is small (see src/api/auth.ts and src/api/internships.ts), so a hand-written
// wrapper stays easy to read end-to-end. Its one job: attach the JWT when present, parse
// the backend's RFC 9457 Problem Details body on failure (see docs/DECISIONS.md D17),
// and throw an ApiError the UI can display directly via .message.

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string

export class ApiError extends Error {
  status: number
  // Field -> messages, present only for 400s from DataAnnotations validation failures.
  errors?: Record<string, string[]>

  constructor(status: number, message: string, errors?: Record<string, string[]>) {
    super(message)
    this.status = status
    this.errors = errors
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  token?: string | null
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = {}
  if (options.body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }
  if (options.token) {
    headers['Authorization'] = `Bearer ${options.token}`
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? 'GET',
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  })

  if (response.status === 204) {
    return undefined as T
  }

  const isJson = response.headers.get('content-type')?.includes('json') ?? false
  const data = isJson ? await response.json() : null

  if (!response.ok) {
    // ProblemDetails (application/problem+json) shape: { title, status, detail, errors? }.
    const detail = data?.detail ?? data?.title ?? `Request failed with status ${response.status}`
    throw new ApiError(response.status, detail, data?.errors)
  }

  return data as T
}
