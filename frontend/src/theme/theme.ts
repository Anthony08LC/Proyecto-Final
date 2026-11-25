import { useCallback, useEffect, useState } from 'react'

/**
 * Tema claro u oscuro (RNF-USA-02): por defecto sigue al sistema operativo; la elección manual se
 * recuerda en el navegador. Es una preferencia de interfaz, no estado del servidor.
 */
export type ThemePreference = 'system' | 'light' | 'dark'

const STORAGE_KEY = 'quoteflow.theme'

function readPreference(): ThemePreference {
  try {
    const value = window.localStorage.getItem(STORAGE_KEY)
    return value === 'light' || value === 'dark' ? value : 'system'
  } catch {
    return 'system'
  }
}

function apply(preference: ThemePreference): void {
  const root = document.documentElement
  if (preference === 'system') {
    root.removeAttribute('data-theme')
  } else {
    root.setAttribute('data-theme', preference)
  }
}

/** Aplica la preferencia guardada antes del primer render, para evitar un parpadeo. */
export function applyStoredTheme(): void {
  apply(readPreference())
}

export function useTheme() {
  const [preference, setPreference] = useState<ThemePreference>(readPreference)

  useEffect(() => {
    apply(preference)
    try {
      if (preference === 'system') {
        window.localStorage.removeItem(STORAGE_KEY)
      } else {
        window.localStorage.setItem(STORAGE_KEY, preference)
      }
    } catch {
      // Sin almacenamiento: la preferencia dura hasta recargar.
    }
  }, [preference])

  const cycle = useCallback(() => {
    setPreference((current) =>
      current === 'system' ? 'light' : current === 'light' ? 'dark' : 'system',
    )
  }, [])

  return { preference, cycle }
}
