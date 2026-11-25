import { useQuery } from '@tanstack/react-query'
import { getHealth } from '../../api/health'

export const healthQueryKey = ['health'] as const

/** Estado del backend y de la base de datos, actualizado cada 30 segundos. */
export function useHealth() {
  return useQuery({
    queryKey: healthQueryKey,
    queryFn: ({ signal }) => getHealth(signal),
    refetchInterval: 30_000,
    retry: false,
  })
}
