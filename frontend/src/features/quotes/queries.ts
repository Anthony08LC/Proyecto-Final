import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getAssistantStatus } from '../../api/assistant'
import { getQuote, getQuotes, type QuoteFilters } from '../../api/quotes'

export const quoteKeys = {
  all: ['quotes'] as const,
  list: (filters: QuoteFilters) => ['quotes', 'list', filters] as const,
  detail: (id: string) => ['quotes', 'detail', id] as const,
}

export function useQuotes(filters: QuoteFilters) {
  return useQuery({
    queryKey: quoteKeys.list(filters),
    queryFn: ({ signal }) => getQuotes(filters, signal),
    placeholderData: keepPreviousData,
  })
}

export function useQuote(id: string) {
  return useQuery({
    queryKey: quoteKeys.detail(id),
    queryFn: ({ signal }) => getQuote(id, signal),
  })
}

export const assistantKeys = {
  status: ['assistant', 'status'] as const,
}

/** Disponibilidad del asistente de IA y cupo del usuario (RF-IA-03, RNF-SEG-12). */
export function useAssistantStatus(enabled: boolean) {
  return useQuery({
    queryKey: assistantKeys.status,
    queryFn: ({ signal }) => getAssistantStatus(signal),
    enabled,
    staleTime: 60_000,
  })
}
