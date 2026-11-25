import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'react-router'

/**
 * Filtros y página de un listado guardados en la URL: se conservan al volver desde un detalle o
 * un formulario y se pueden compartir. Cambiar un filtro vuelve a la página 1.
 * `keys` debe ser una constante del módulo que llama, para que la referencia sea estable.
 */
export function useListParams<K extends string>(keys: readonly K[]) {
  const [params, setParams] = useSearchParams()

  const values = useMemo(() => {
    const result = {} as Record<K, string>
    for (const key of keys) {
      result[key] = params.get(key) ?? ''
    }
    return result
  }, [params, keys])

  const page = Math.max(1, Number.parseInt(params.get('page') ?? '1', 10) || 1)

  const setFilter = useCallback(
    (key: K, value: string) => {
      setParams(
        (current) => {
          const next = new URLSearchParams(current)
          if (value) {
            next.set(key, value)
          } else {
            next.delete(key)
          }
          next.delete('page')
          return next
        },
        { replace: true },
      )
    },
    [setParams],
  )

  const setPage = useCallback(
    (nextPage: number) => {
      setParams((current) => {
        const next = new URLSearchParams(current)
        if (nextPage <= 1) {
          next.delete('page')
        } else {
          next.set('page', String(nextPage))
        }
        return next
      })
    },
    [setParams],
  )

  /** Quita todos los filtros y vuelve a la página 1, en una sola navegación. */
  const clearFilters = useCallback(() => {
    setParams(
      (current) => {
        const next = new URLSearchParams(current)
        for (const key of keys) {
          next.delete(key)
        }
        next.delete('page')
        return next
      },
      { replace: true },
    )
  }, [setParams, keys])

  return { values, page, setFilter, setPage, clearFilters }
}

/** Filtro de tres estados de la API (`true`, `false` o sin filtro) a partir de la URL. */
export function toBooleanFilter(value: string): boolean | undefined {
  if (value === 'true') {
    return true
  }
  if (value === 'false') {
    return false
  }
  return undefined
}
