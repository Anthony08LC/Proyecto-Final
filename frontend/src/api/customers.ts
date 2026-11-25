import { request } from './http'
import type { QuoteSummary } from './quotes'
import type { PagedResult } from './types'

/** Cliente en listados (`CustomerSummaryDto`). */
export interface CustomerSummary {
  id: string
  name: string
  company: string | null
  email: string | null
  /** Normalizado como `+506XXXXXXXX`. */
  phone: string | null
  isVip: boolean
  isActive: boolean
}

/** Detalle de un cliente (`CustomerDto`). */
export interface Customer extends CustomerSummary {
  notes: string | null
  createdAt: string
  updatedAt: string
}

/**
 * Resultado de registrar o editar (`CustomerSaveResult`). `possibleDuplicateEmail` advierte que
 * otro cliente ya usa el correo; la operación se completa igual (RF-CLI-01).
 */
export interface CustomerSaveResult {
  customerId: string
  possibleDuplicateEmail: boolean
}

export interface CustomerFilters {
  search?: string | undefined
  isActive?: boolean | undefined
  isVip?: boolean | undefined
  page?: number | undefined
  pageSize?: number | undefined
}

export interface CustomerInput {
  name: string
  company: string | null
  email: string | null
  phone: string | null
  notes: string | null
}

/** Límites del dominio (`Customer`). */
export const CUSTOMER_NAME_MAX = 150
export const CUSTOMER_COMPANY_MAX = 150
export const CUSTOMER_NOTES_MAX = 1000

/** Busca por nombre, empresa o correo, ordenado por nombre (RF-CLI-02). Todos los roles. */
export function getCustomers(filters: CustomerFilters, signal?: AbortSignal) {
  return request<PagedResult<CustomerSummary>>('/api/customers', { query: { ...filters }, signal })
}

/** Detalle (RF-CLI-03). Todos los roles. */
export function getCustomer(id: string, signal?: AbortSignal) {
  return request<Customer>(`/api/customers/${id}`, { signal })
}

/** Cotizaciones del cliente, más recientes primero (RF-CLI-03). */
export function getCustomerQuotes(id: string, page: number, signal?: AbortSignal) {
  return request<PagedResult<QuoteSummary>>(`/api/customers/${id}/quotes`, {
    query: { page },
    signal,
  })
}

/** Registra un cliente (RF-CLI-01). Ventas y Administrador; solo este último puede indicar VIP. */
export function createCustomer(input: CustomerInput & { isVip: boolean }) {
  return request<CustomerSaveResult>('/api/customers', { method: 'POST', body: input })
}

/** Solo Administrador (RF-CLI-04). */
export function updateCustomer(id: string, input: CustomerInput) {
  return request<CustomerSaveResult>(`/api/customers/${id}`, { method: 'PUT', body: input })
}

/** Solo Administrador (RF-CLI-05). */
export function markCustomerAsVip(id: string) {
  return request<void>(`/api/customers/${id}/vip`, { method: 'PUT' })
}

/** Solo Administrador (RF-CLI-05). */
export function removeCustomerVip(id: string) {
  return request<void>(`/api/customers/${id}/vip`, { method: 'DELETE' })
}

/** Solo Administrador (RF-CLI-06). */
export function deactivateCustomer(id: string) {
  return request<void>(`/api/customers/${id}/deactivate`, { method: 'POST' })
}

/** Solo Administrador (RF-CLI-06). */
export function reactivateCustomer(id: string) {
  return request<void>(`/api/customers/${id}/reactivate`, { method: 'POST' })
}
