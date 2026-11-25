import type { PagedResult } from '../../api/types'
import { Button } from './Button'

interface PaginationProps {
  result: Pick<PagedResult<unknown>, 'page' | 'totalPages' | 'totalCount' | 'pageSize'>
  onPageChange: (page: number) => void
}

/** Paginación de los listados (RNF-REN-05). */
export function Pagination({ result, onPageChange }: PaginationProps) {
  const { page, totalPages, totalCount, pageSize } = result
  if (totalCount === 0) {
    return null
  }

  const first = (page - 1) * pageSize + 1
  const last = Math.min(page * pageSize, totalCount)

  return (
    <nav className="pagination" aria-label="Paginación">
      <span className="muted">
        {first}–{last} de {totalCount}
      </span>
      <div className="pagination__buttons">
        <Button onClick={() => onPageChange(page - 1)} disabled={page <= 1}>
          Anterior
        </Button>
        <span className="muted">
          Página {page} de {Math.max(totalPages, 1)}
        </span>
        <Button onClick={() => onPageChange(page + 1)} disabled={page >= totalPages}>
          Siguiente
        </Button>
      </div>
    </nav>
  )
}
