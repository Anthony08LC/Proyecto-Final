import { request } from './http'
import type { CreatedResponse, PagedResult } from './types'

/** Producto del catálogo con su categoría (`ProductDto`). */
export interface Product {
  id: string
  name: string
  description: string | null
  categoryId: string
  categoryName: string
  categoryIsActive: boolean
  /** Precio vigente en colones (solo para mostrar). */
  price: number
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface ProductFilters {
  search?: string | undefined
  categoryId?: string | undefined
  isActive?: boolean | undefined
  page?: number | undefined
  pageSize?: number | undefined
}

export interface ProductInput {
  name: string
  description: string | null
  categoryId: string
  price: number
}

/** Límites del dominio (`Product`). */
export const PRODUCT_NAME_MAX = 150
export const PRODUCT_DESCRIPTION_MAX = 1000

/** Busca en el catálogo (RF-PRO-02). Todos los roles. */
export function getProducts(filters: ProductFilters, signal?: AbortSignal) {
  return request<PagedResult<Product>>('/api/products', { query: { ...filters }, signal })
}

export function getProduct(id: string, signal?: AbortSignal) {
  return request<Product>(`/api/products/${id}`, { signal })
}

/** Solo Administrador (RF-PRO-01). 409 si el nombre ya existe. */
export async function createProduct(input: ProductInput): Promise<string> {
  const created = await request<CreatedResponse>('/api/products', { method: 'POST', body: input })
  return created.id
}

/** Solo Administrador (RF-PRO-03). Un cambio de precio queda en la auditoría. */
export function updateProduct(id: string, input: ProductInput) {
  return request<void>(`/api/products/${id}`, { method: 'PUT', body: input })
}

/** Solo Administrador (RF-PRO-04). */
export function deactivateProduct(id: string) {
  return request<void>(`/api/products/${id}/deactivate`, { method: 'POST' })
}

/** Solo Administrador (RF-PRO-04). */
export function reactivateProduct(id: string) {
  return request<void>(`/api/products/${id}/reactivate`, { method: 'POST' })
}
