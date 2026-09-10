import axios, {
  AxiosError,
  type InternalAxiosRequestConfig,
} from 'axios'
import {
  clearAuthSession,
  getAuthSession,
  saveAuthSession,
} from '../auth/authStorage'
import { refreshRequest } from './authApi'
import type { TokenResponse } from '../types/auth'

interface RetryableRequestConfig
  extends InternalAxiosRequestConfig {
  _retry?: boolean
}

export const httpClient =
  axios.create({
    headers: {
      Accept: 'application/json',
    },
  })

let refreshPromise:
  Promise<TokenResponse> | null = null

httpClient.interceptors.request.use(
  (config) => {
    const session =
      getAuthSession()

    if (session?.accessToken) {
      config.headers.Authorization =
        `Bearer ${session.accessToken}`
    }

    return config
  },
)

httpClient.interceptors.response.use(
  (response) => response,

  async (error: AxiosError) => {
    const originalRequest =
      error.config as
        | RetryableRequestConfig
        | undefined

    if (
      error.response?.status !== 401 ||
      !originalRequest ||
      originalRequest._retry
    ) {
      return Promise.reject(error)
    }

    const currentSession =
      getAuthSession()

    if (!currentSession?.refreshToken) {
      clearAuthSession()
      window.location.assign('/login')

      return Promise.reject(error)
    }

    originalRequest._retry = true

    try {
      refreshPromise ??=
        refreshRequest(
          currentSession.refreshToken,
        )

      const tokens =
        await refreshPromise

      const latestSession =
        getAuthSession()

      if (!latestSession) {
        throw new Error(
          'Authentication session is no longer available.',
        )
      }

      saveAuthSession({
        ...latestSession,
        accessToken:
          tokens.accessToken,
        refreshToken:
          tokens.refreshToken,
        expiresAt:
          tokens.expiresAt,
      })

      originalRequest.headers.Authorization =
        `Bearer ${tokens.accessToken}`

      return await httpClient(
        originalRequest,
      )
    } catch (refreshError) {
      clearAuthSession()
      window.location.assign('/login')

      return Promise.reject(
        refreshError,
      )
    } finally {
      refreshPromise = null
    }
  },
)