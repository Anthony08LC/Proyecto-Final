import { request } from './http'
import type { CreatedResponse, PagedResult } from './types'

/** Categoría con la cantidad de productos que la usan (`CategoryDto`, RF-CAT-02). */
export interface Category {
  id: string
  name: string
  description: string | null
  isActive: boolean
  productCount: number
  createdAt: string
  updatedAt: string
}

export interface CategoryFilters {
  search?: string | undefined
  isActive?: boolean | undefined
  page?: number | undefined
  pageSize?: number | undefined
}

export interface CategoryInput {
  name: string
  description: string | null
}

/** Límites del dominio (`Category`). */
export const CATEGORY_NAME_MAX = 80
export const CATEGORY_DESCRIPTION_MAX = 500

/** Lista las categorías. Disponible para todos los roles (filtro del catálogo). */
export function getCategories(filters: CategoryFilters, signal?: AbortSignal) {
  return request<PagedResult<Category>>('/api/categories', { query: { ...filters }, signal })
}

export function getCategory(id: string, signal?: AbortSignal) {
  return request<Category>(`/api/categories/${id}`, { signal })
}

/** Solo Administrador (RF-CAT-01). 409 si el nombre ya existe, activo o inactivo. */
export async function createCategory(input: CategoryInput): Promise<string> {
  const created = await request<CreatedResponse>('/api/categories', { method: 'POST', body: input })
  return created.id
}

/** Solo Administrador (RF-CAT-03). */
export function updateCategory(id: string, input: CategoryInput) {
  return request<void>(`/api/categories/${id}`, { method: 'PUT', body: input })
}

/** Solo Administrador (RF-CAT-04). */
export function deactivateCategory(id: string) {
  return request<void>(`/api/categories/${id}/deactivate`, { method: 'POST' })
}

/** Solo Administrador (RF-CAT-04). */
export function reactivateCategory(id: string) {
  return request<void>(`/api/categories/${id}/reactivate`, { method: 'POST' })
}
