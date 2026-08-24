import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import type { AuthResponse, UserRole } from '../types'

const STORAGE_KEY = 'ims_auth'

interface StoredAuth {
  token: string
  email: string
  role: UserRole
  expiresAt: string
}

interface AuthContextValue {
  token: string | null
  email: string | null
  role: UserRole | null
  isAuthenticated: boolean
  login: (auth: AuthResponse) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function readStoredAuth(): StoredAuth | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) return null
  try {
    const stored = JSON.parse(raw) as StoredAuth
    // Tokens are stateless (see docs/LEARNING_LOG.md Phase 11) - the backend can't tell
    // us a token expired, so the frontend checks the expiry it was issued with, rather
    // than showing "logged in" and then having every request fail with 401.
    if (new Date(stored.expiresAt) <= new Date()) {
      localStorage.removeItem(STORAGE_KEY)
      return null
    }
    return stored
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<StoredAuth | null>(() => readStoredAuth())

  const value = useMemo<AuthContextValue>(
    () => ({
      token: auth?.token ?? null,
      email: auth?.email ?? null,
      role: auth?.role ?? null,
      isAuthenticated: auth !== null,
      login: (response: AuthResponse) => {
        const stored: StoredAuth = {
          token: response.token,
          email: response.email,
          role: response.role,
          expiresAt: response.expiresAt,
        }
        localStorage.setItem(STORAGE_KEY, JSON.stringify(stored))
        setAuth(stored)
      },
      logout: () => {
        localStorage.removeItem(STORAGE_KEY)
        setAuth(null)
      },
    }),
    [auth],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
