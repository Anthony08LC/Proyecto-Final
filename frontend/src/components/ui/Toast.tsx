import { useCallback, useMemo, useState, type ReactNode } from 'react'
import { ToastContext, type ToastTone } from './toastContext'

/** Notificaciones breves tras una acción (guardado, desactivación, avisos). */

interface ToastItem {
  id: number
  tone: ToastTone
  message: string
}

const DURATION_MS = 5000
let nextId = 1

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([])

  const dismiss = useCallback((id: number) => {
    setToasts((current) => current.filter((toast) => toast.id !== id))
  }, [])

  const notify = useCallback(
    (message: string, tone: ToastTone = 'success') => {
      const id = nextId++
      setToasts((current) => [...current, { id, tone, message }])
      // Los avisos duran el doble: suelen pedir que el usuario revise algo.
      window.setTimeout(() => dismiss(id), tone === 'success' ? DURATION_MS : DURATION_MS * 2)
    },
    [dismiss],
  )

  const value = useMemo(() => ({ notify }), [notify])

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toasts" aria-live="polite">
        {toasts.map((toast) => (
          <div key={toast.id} className={`toast toast--${toast.tone}`} role="status">
            <span>{toast.message}</span>
            <button
              type="button"
              className="toast__close"
              aria-label="Cerrar aviso"
              onClick={() => dismiss(toast.id)}
            >
              ×
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}
