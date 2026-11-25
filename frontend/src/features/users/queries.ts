import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { MAX_PAGE_SIZE } from '../../api/types'
import { getUser, getUsers, type UserFilters } from '../../api/users'

export const userKeys = {
  all: ['users'] as const,
  list: (filters: UserFilters) => ['users', 'list', filters] as const,
  detail: (id: string) => ['users', 'detail', id] as const,
}

export function useUsers(filters: UserFilters) {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: ({ signal }) => getUsers(filters, signal),
    placeholderData: keepPreviousData,
  })
}

export function useUser(id: string) {
  return useQuery({
    queryKey: userKeys.detail(id),
    queryFn: ({ signal }) => getUser(id, signal),
  })
}

/** Todos los usuarios (activos e inactivos) para filtros. Son pocos: una página de 100 alcanza. */
export function useUserOptions({ enabled = true }: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: userKeys.list({ pageSize: MAX_PAGE_SIZE }),
    queryFn: ({ signal }) => getUsers({ pageSize: MAX_PAGE_SIZE }, signal),
    enabled,
  })
}
