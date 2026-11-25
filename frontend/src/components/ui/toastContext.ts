import { createContext, useContext } from 'react'

export type ToastTone = 'success' | 'warning' | 'danger'

export interface ToastContextValue {
  notify: (message: string, tone?: ToastTone) => void
}

export const ToastContext = createContext<ToastContextValue | null>(null)

/** Muestra una notificación breve tras una acción (guardado, desactivación, avisos). */
export function useToast(): ToastContextValue {
  const context = useContext(ToastContext)
  if (!context) {
    throw new Error('useToast debe usarse dentro de ToastProvider.')
  }
  return context
}
