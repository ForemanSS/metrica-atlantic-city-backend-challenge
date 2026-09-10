import type { AuthSession } from '../types/auth'

const STORAGE_KEY = 'atlantic-city.auth'

export function getAuthSession(): AuthSession | null {
  const value = sessionStorage.getItem(STORAGE_KEY)

  if (!value) {
    return null
  }

  try {
    return JSON.parse(value) as AuthSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveAuthSession(session: AuthSession): void {
  sessionStorage.setItem(
    STORAGE_KEY,
    JSON.stringify(session),
  )
}

export function clearAuthSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}