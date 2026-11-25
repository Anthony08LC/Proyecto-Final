import { createContext } from 'react'
import type { AccessToken, UserProfile } from '../api/auth'
import type { StoredSession } from './sessionStorage'

/** Motivo por el que terminó la sesión, para avisar al usuario en el inicio de sesión. */
export type EndReason = 'logout' | 'expired'

export interface SessionContextValue {
  user: UserProfile | null
  isAdministrator: boolean
  endReason: EndReason | null
  start: (session: StoredSession) => void
  /** Reemplaza el token (por ejemplo, tras cambiar la contraseña) y actualiza el perfil. */
  renew: (token: AccessToken, user: UserProfile) => void
  updateUser: (user: UserProfile) => void
  end: (reason: EndReason) => void
}

export const SessionContext = createContext<SessionContextValue | null>(null)
