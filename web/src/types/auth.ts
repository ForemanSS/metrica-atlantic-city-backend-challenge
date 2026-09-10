export interface AuthUser {
  id: string
  email: string
  displayName: string
  role: string
  permissions: string[]
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  tokenType: string
  expiresAt: string
  user: AuthUser
}

export interface TokenResponse {
  accessToken: string
  refreshToken: string
  tokenType: string
  expiresAt: string
}

export interface AuthSession {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: AuthUser
}