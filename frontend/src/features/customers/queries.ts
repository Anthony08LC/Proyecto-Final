import { keepPreviousData, useQuery } from '@tanstack/react-query'
import {
  getCustomer,
  getCustomerQuotes,
  getCustomers,
  type CustomerFilters,
} from '../../api/customers'
import { EMAIL_PATTERN } from '../../lib/forms'

export const customerKeys = {
  all: ['customers'] as const,
  list: (filters: CustomerFilters) => ['customers', 'list', filters] as const,
  detail: (id: string) => ['customers', 'detail', id] as const,
  quotes: (id: string, page: number) => ['customers', 'detail', id, 'quotes', page] as const,
}

export function useCustomers(filters: CustomerFilters) {
  return useQuery({
    queryKey: customerKeys.list(filters),
    queryFn: ({ signal }) => getCustomers(filters, signal),
    placeholderData: keepPreviousData,
  })
}

export function useCustomer(id: string) {
  return useQuery({
    queryKey: customerKeys.detail(id),
    queryFn: ({ signal }) => getCustomer(id, signal),
  })
}

export function useCustomerQuotes(id: string, page: number) {
  return useQuery({
    queryKey: customerKeys.quotes(id, page),
    queryFn: ({ signal }) => getCustomerQuotes(id, page, signal),
    placeholderData: keepPreviousData,
  })
}

/**
 * Otro cliente que ya usa el correo escrito (sin distinguir mayúsculas), para advertir de un
 * posible duplicado mientras se escribe (RF-CLI-01). El backend repite la verificación al guardar.
 */
export function useCustomerWithEmail(email: string, excludingId: string | undefined) {
  const normalized = email.trim().toLowerCase()

  return useQuery({
    queryKey: customerKeys.list({ search: normalized, pageSize: 10 }),
    queryFn: ({ signal }) => getCustomers({ search: normalized, pageSize: 10 }, signal),
    enabled: EMAIL_PATTERN.test(normalized),
    select: (result) =>
      result.items.find(
        (customer) => customer.email === normalized && customer.id !== excludingId,
      ) ?? null,
  })
}
