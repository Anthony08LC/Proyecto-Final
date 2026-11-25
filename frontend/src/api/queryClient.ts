import { QueryClient } from '@tanstack/react-query'
import { ApiError } from './apiError'

/**
 * Caché de datos del servidor (spec 3.1, DT-03). Solo se reintentan los fallos que pueden ser
 * pasajeros (sin conexión o error del servidor), nunca los 4xx: repetir una solicitud inválida o
 * sin permiso no cambia el resultado.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      retry: (failureCount, error) => failureCount < 2 && isTransient(error),
    },
    mutations: {
      retry: false,
    },
  },
})

function isTransient(error: unknown): boolean {
  return error instanceof ApiError && (error.isNetworkError || error.status >= 500)
}
