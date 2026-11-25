/** Tipos compartidos con la API (los enums viajan como texto). */

export type UserRole = 'Administrator' | 'Sales'

export type QuoteStatus = 'Draft' | 'Issued' | 'Accepted' | 'Rejected' | 'Expired' | 'Cancelled'

/** Página de resultados (`PagedResult<T>` del backend). */
export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

/** Respuesta de una creación: el identificador del recurso nuevo. */
export interface CreatedResponse {
  id: string
}

/** Tamaño de página por defecto de todos los listados (RNF-REN-05). */
export const DEFAULT_PAGE_SIZE = 20

/** Máximo que acepta la API por página. */
export const MAX_PAGE_SIZE = 100
