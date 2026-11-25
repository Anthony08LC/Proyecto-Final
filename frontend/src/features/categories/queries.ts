import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getCategories, getCategory, type CategoryFilters } from '../../api/categories'
import { MAX_PAGE_SIZE } from '../../api/types'

export const categoryKeys = {
  all: ['categories'] as const,
  list: (filters: CategoryFilters) => ['categories', 'list', filters] as const,
  detail: (id: string) => ['categories', 'detail', id] as const,
}

export function useCategories(filters: CategoryFilters) {
  return useQuery({
    queryKey: categoryKeys.list(filters),
    queryFn: ({ signal }) => getCategories(filters, signal),
    placeholderData: keepPreviousData,
  })
}

export function useCategory(id: string) {
  return useQuery({
    queryKey: categoryKeys.detail(id),
    queryFn: ({ signal }) => getCategory(id, signal),
  })
}

/**
 * Categorías para selectores y filtros. Con `onlyActive`, solo las asignables a un producto
 * (RN-29). El catálogo tiene pocas categorías: una página de 100 alcanza.
 */
export function useCategoryOptions({ onlyActive }: { onlyActive: boolean }) {
  return useCategories({ isActive: onlyActive ? true : undefined, pageSize: MAX_PAGE_SIZE })
}

/**
 * Categoría inactiva con el mismo nombre (sin distinguir mayúsculas) que el escrito, para sugerir
 * reactivarla en lugar de crear una nueva (RF-CAT-01).
 */
export function useInactiveCategoryNamed(name: string) {
  const normalized = name.trim().toLocaleLowerCase('es')

  return useQuery({
    queryKey: categoryKeys.list({ search: normalized, isActive: false, pageSize: 5 }),
    queryFn: ({ signal }) =>
      getCategories({ search: normalized, isActive: false, pageSize: 5 }, signal),
    enabled: normalized.length > 0,
    select: (result) =>
      result.items.find((category) => category.name.toLocaleLowerCase('es') === normalized) ?? null,
  })
}
