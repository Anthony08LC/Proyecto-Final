import { request } from './http'
import type { UserRole } from './types'

/** Perfil del usuario autenticado (`UserProfileDto`). */
export interface UserProfile {
  id: string
  fullName: string
  email: string
  role: UserRole
  mustChangePassword: boolean
}

export interface AccessToken {
  token: string
  expiresAt: string
}

export interface LoginResult extends AccessToken {
  user: UserProfile
}

/** Inicia sesión (RF-AUT-01). Endpoint anónimo, limitado a 5 intentos por minuto (RNF-SEG-05). */
export function login(email: string, password: string): Promise<LoginResult> {
  return request<LoginResult>('/api/auth/login', { method: 'POST', body: { email, password } })
}

/**
 * Cambia la contraseña propia (RF-AUT-06, RF-AUT-07). Invalida las demás sesiones y devuelve un
 * token nuevo para la actual.
 */
export function changePassword(currentPassword: string, newPassword: string): Promise<AccessToken> {
  return request<AccessToken>('/api/auth/change-password', {
    method: 'POST',
    body: { currentPassword, newPassword },
  })
}
