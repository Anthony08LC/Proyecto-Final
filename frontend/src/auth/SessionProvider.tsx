import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { queryClient } from '../api/queryClient'
import { configureHttp } from '../api/http'
import type { AccessToken, UserProfile } from '../api/auth'
import { SessionContext, type EndReason, type SessionContextValue } from './sessionContext'
import { clearSession, loadSession, saveSession, type StoredSession } from './sessionStorage'

/**
 * Estado global de la sesión: el único estado global junto con las preferencias de interfaz
 * (spec 3.1). Conecta la capa HTTP con el token y cierra la sesión ante un 401 (RF-AUT-03) o
 * al vencer el token.
 */
export function SessionProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<StoredSession | null>(() => loadSession())
  const [endReason, setEndReason] = useState<EndReason | null>(null)

  const end = useCallback((reason: EndReason) => {
    clearSession()
    queryClient.clear()
    setEndReason(reason)
    setSession(null)
  }, [])

  const start = useCallback((next: StoredSession) => {
    queryClient.clear()
    saveSession(next)
    setEndReason(null)
    setSession(next)
  }, [])

  const renew = useCallback((token: AccessToken, user: UserProfile) => {
    const next = { token: token.token, expiresAt: token.expiresAt, user }
    saveSession(next)
    setSession(next)
  }, [])

  const updateUser = useCallback((user: UserProfile) => {
    setSession((current) => {
      if (!current) {
        return current
      }
      const next = { ...current, user }
      saveSession(next)
      return next
    })
  }, [])

  // La capa HTTP lee el token actual y avisa cuando la API rechaza la sesión. Se configura en el
  // render (es idempotente) y no en un efecto: los efectos de los hijos, que lanzan las primeras
  // consultas, se ejecutan antes que los del padre y saldrían sin token.
  const token = session?.token ?? null
  configureHttp({
    getToken: () => token,
    onUnauthorized: () => end('expired'),
  })

  // Cierre automático al vencer el token (8 horas, sin refresh tokens).
  const expiresAt = session?.expiresAt
  useEffect(() => {
    if (!expiresAt) {
      return undefined
    }
    const remaining = Date.parse(expiresAt) - Date.now()
    const timer = window.setTimeout(() => end('expired'), Math.max(remaining, 0))
    return () => window.clearTimeout(timer)
  }, [expiresAt, end])

  const value = useMemo<SessionContextValue>(
    () => ({
      user: session?.user ?? null,
      isAdministrator: session?.user.role === 'Administrator',
      endReason,
      start,
      renew,
      updateUser,
      end,
    }),
    [session, endReason, start, renew, updateUser, end],
  )

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}
