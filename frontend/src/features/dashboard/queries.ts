import { useQuery } from '@tanstack/react-query'
import { getDashboard } from '../../api/dashboard'

export const dashboardKeys = { all: ['dashboard'] as const }

export function useDashboard() {
  return useQuery({
    queryKey: dashboardKeys.all,
    queryFn: ({ signal }) => getDashboard(signal),
    // Es la página de inicio: se actualiza en cada visita.
    staleTime: 0,
  })
}
