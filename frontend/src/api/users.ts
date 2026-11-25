import { request } from './http'
import type { PagedResult, UserRole } from './types'

/** Usuario del sistema (`UserDto`). */
export interface User {
  id: string
  fullName: string
  email: string
  role: UserRole
  isActive: boolean
  mustChangePassword: boolean
  createdAt: string
  updatedAt: string
}

export interface UserFilters {
  search?: string | undefined
  role?: UserRole | undefined
  isActive?: boolean | undefined
  page?: number | undefined
  pageSize?: number | undefined
}

/**
 * Lista los usuarios (RF-USR-01). Solo Administrador. Por ahora se usa para el filtro de
 * propietario del historial de cotizaciones; la gestión de usuarios llega en su propio módulo.
 */
export function getUsers(filters: UserFilters, signal?: AbortSignal) {
  return request<PagedResult<User>>('/api/users', { query: { ...filters }, signal })
}
