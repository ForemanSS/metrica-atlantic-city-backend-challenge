import axios from 'axios'
import type {
  LoginResponse,
  TokenResponse,
} from '../types/auth'

export async function loginRequest(
  email: string,
  password: string,
): Promise<LoginResponse> {
  const response =
    await axios.post<LoginResponse>(
      '/auth/login',
      {
        email,
        password,
      },
    )

  return response.data
}

export async function refreshRequest(
  refreshToken: string,
): Promise<TokenResponse> {
  const response =
    await axios.post<TokenResponse>(
      '/auth/refresh',
      {
        refreshToken,
      },
    )

  return response.data
}

export async function revokeRequest(
  refreshToken: string,
): Promise<void> {
  await axios.post(
    '/auth/revoke',
    {
      refreshToken,
    },
  )
}