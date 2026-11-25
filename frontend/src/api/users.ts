import { request } from './http'
import type { CreatedResponse, PagedResult, UserRole } from './types'

/** Usuario del sistema (`UserDto`). Nunca incluye la contraseña. */
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

/** Nombre, correo y rol: lo que el Administrador edita (RF-USR-03). */
export interface UserInput {
  fullName: string
  email: string
  role: UserRole
}

/** Límite del dominio (`User`). */
export const USER_FULL_NAME_MAX = 150

/** Lista y busca usuarios (RF-USR-01). Solo Administrador. El usuario de sistema no aparece. */
export function getUsers(filters: UserFilters, signal?: AbortSignal) {
  return request<PagedResult<User>>('/api/users', { query: { ...filters }, signal })
}

export function getUser(id: string, signal?: AbortSignal) {
  return request<User>(`/api/users/${id}`, { signal })
}

/** Alta con contraseña temporal (RF-USR-02). 409 si el correo ya existe, aunque esté inactivo. */
export async function createUser(
  input: UserInput & { temporaryPassword: string },
): Promise<string> {
  const created = await request<CreatedResponse>('/api/users', { method: 'POST', body: input })
  return created.id
}

/** Edición (RF-USR-03). 409 si el correo está en uso o se quita el rol al último Administrador. */
export function updateUser(id: string, input: UserInput) {
  return request<void>(`/api/users/${id}`, { method: 'PUT', body: input })
}

/** RF-USR-04. 409 si es uno mismo o el último Administrador activo (RN-31). */
export function deactivateUser(id: string) {
  return request<void>(`/api/users/${id}/deactivate`, { method: 'POST' })
}

/** RF-USR-04. */
export function reactivateUser(id: string) {
  return request<void>(`/api/users/${id}/reactivate`, { method: 'POST' })
}

/** Asigna una contraseña temporal (RF-USR-05); el usuario deberá cambiarla al entrar. */
export function resetUserPassword(id: string, temporaryPassword: string) {
  return request<void>(`/api/users/${id}/reset-password`, {
    method: 'POST',
    body: { temporaryPassword },
  })
}
