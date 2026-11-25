import { keepPreviousData, useQuery } from '@tanstack/react-query'
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
