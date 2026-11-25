import { Link } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import { deactivateCategory, reactivateCategory } from '../../api/categories'
import { useActivationDialog } from '../../components/ActivationDialog'
import { Button, ButtonLink } from '../../components/ui/Button'
import { ActiveBadge, Alert, EmptyState, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { Pagination } from '../../components/ui/Pagination'
import { toBooleanFilter, useListParams } from '../../lib/useListParams'
import { categoryKeys, useCategories } from './queries'

const FILTER_KEYS = ['status'] as const

/** Listado de categorías con su estado y cantidad de productos (RF-CAT-02). Solo Administrador. */
export function CategoriesPage() {
  const { values, page, setFilter, setPage } = useListParams(FILTER_KEYS)
  const { data, error, isPending, isPlaceholderData } = useCategories({
    isActive: toBooleanFilter(values.status),
    page,
  })

  const activation = useActivationDialog({
    entityLabel: 'la categoría',
    deactivate: deactivateCategory,
    reactivate: reactivateCategory,
    invalidate: categoryKeys.all,
    deactivateConsequences: (
      <>
        <p>No se podrá asignar a productos nuevos ni editados.</p>
        <p>Los productos que ya la tienen no cambian. Puede reactivarla cuando quiera.</p>
      </>
    ),
    reactivateConsequences: <p>Volverá a estar disponible para asignarla a productos.</p>,
  })

  return (
    <>
      <PageHeader
        title="Categorías"
        description="Clasificación del catálogo de productos."
        actions={
          <ButtonLink to="/categories/new" variant="primary">
            Nueva categoría
          </ButtonLink>
        }
      />

      <div className="toolbar">
        <select
          className="input input--select"
          aria-label="Filtrar por estado"
          value={values.status}
          onChange={(event) => setFilter('status', event.target.value)}
        >
          <option value="">Todos los estados</option>
          <option value="true">Activas</option>
          <option value="false">Inactivas</option>
        </select>
      </div>

      {error && <Alert tone="danger">{getErrorMessage(error)}</Alert>}
      {isPending && <Spinner />}

      {data &&
        (data.items.length === 0 ? (
          <EmptyState title="No hay categorías">
            <p>No se encontraron categorías con el filtro seleccionado.</p>
          </EmptyState>
        ) : (
          <>
            <div className="table-wrapper" aria-busy={isPlaceholderData}>
              <table className="table">
                <thead>
                  <tr>
                    <th scope="col">Nombre</th>
                    <th scope="col" className="numeric">
                      Productos
                    </th>
                    <th scope="col">Estado</th>
                    <th scope="col" className="actions">
                      <span className="visually-hidden">Acciones</span>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((category) => (
                    <tr key={category.id}>
                      <td>
                        <Link className="table__primary" to={`/categories/${category.id}/edit`}>
                          {category.name}
                        </Link>
                        {category.description && (
                          <span className="table__secondary">{category.description}</span>
                        )}
                      </td>
                      <td className="numeric">{category.productCount}</td>
                      <td>
                        <ActiveBadge isActive={category.isActive} feminine />
                      </td>
                      <td className="actions">
                        <ButtonLink
                          to={`/categories/${category.id}/edit`}
                          className="button--small"
                        >
                          Editar
                        </ButtonLink>
                        <Button
                          className="button--small"
                          variant={category.isActive ? 'ghost' : 'secondary'}
                          onClick={() => activation.request(category)}
                        >
                          {category.isActive ? 'Desactivar' : 'Reactivar'}
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <Pagination result={data} onPageChange={setPage} />
          </>
        ))}

      {activation.dialog}
    </>
  )
}
