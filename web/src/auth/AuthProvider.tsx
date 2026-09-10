import {
  type PropsWithChildren,
  useState,
} from 'react'
import {
  loginRequest,
  revokeRequest,
} from '../api/authApi'
import type {
  AuthSession,
} from '../types/auth'
import {
  clearAuthSession,
  getAuthSession,
  saveAuthSession,
} from './authStorage'
import { AuthContext } from './AuthContext'

export function AuthProvider({
  children,
}: PropsWithChildren) {
  const [session, setSession] =
    useState<AuthSession | null>(
      () => getAuthSession(),
    )

  async function login(
    email: string,
    password: string,
  ): Promise<void> {
    const response =
      await loginRequest(
        email,
        password,
      )

    const nextSession: AuthSession = {
      accessToken:
        response.accessToken,
      refreshToken:
        response.refreshToken,
      expiresAt:
        response.expiresAt,
      user:
        response.user,
    }

    saveAuthSession(
      nextSession,
    )

    setSession(
      nextSession,
    )
  }

  async function logout(): Promise<void> {
    const currentSession =
      getAuthSession()

    try {
      if (currentSession?.refreshToken) {
        await revokeRequest(
          currentSession.refreshToken,
        )
      }
    } finally {
      clearAuthSession()
      setSession(null)
    }
  }

  return (
    <AuthContext.Provider
      value={{
        session,
        isAuthenticated:
          session !== null,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}