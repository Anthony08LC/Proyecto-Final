import type { UserProfile } from '../api/auth'

/**
 * Persistencia de la sesión en el navegador. El JWT dura 8 horas y no hay refresh tokens
 * (RNF-SEG-04); al cerrar sesión se elimina (RF-AUT-02).
 */

export interface StoredSession {
  token: string
  expiresAt: string
  user: UserProfile
}

const STORAGE_KEY = 'quoteflow.session'

export function loadSession(): StoredSession | null {
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY)
    if (!raw) {
      return null
    }

    const session = JSON.parse(raw) as Partial<StoredSession>
    if (
      typeof session.token !== 'string' ||
      typeof session.expiresAt !== 'string' ||
      typeof session.user?.id !== 'string'
    ) {
      return null
    }

    return isExpired(session.expiresAt) ? null : (session as StoredSession)
  } catch {
    return null
  }
}

export function saveSession(session: StoredSession): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
  } catch {
    // Sin almacenamiento (modo privado estricto): la sesión vive solo en memoria.
  }
}

export function clearSession(): void {
  try {
    window.localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Nada que limpiar.
  }
}

export function isExpired(expiresAt: string): boolean {
  const time = Date.parse(expiresAt)
  return Number.isNaN(time) || time <= Date.now()
}
