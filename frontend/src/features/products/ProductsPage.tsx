import { Link } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import { deactivateProduct, reactivateProduct } from '../../api/products'
import { useSession } from '../../auth/useSession'
import { useActivationDialog } from '../../components/ActivationDialog'
import { Button, ButtonLink } from '../../components/ui/Button'
import { ActiveBadge, Alert, Badge, EmptyState, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { Pagination } from '../../components/ui/Pagination'
import { SearchInput } from '../../components/ui/SearchInput'
import { formatCurrency } from '../../lib/format'
import { toBooleanFilter, useListParams } from '../../lib/useListParams'
import { categoryKeys, useCategoryOptions } from '../categories/queries'
import { productKeys, useProducts } from './queries'

const FILTER_KEYS = ['search', 'category', 'status'] as const

/**
 * Catálogo de productos (RF-PRO-02): búsqueda por nombre y filtros por categoría y estado.
 * Ventas lo consulta en modo de solo lectura; la gestión es del Administrador (RF-PRO-01, 03, 04).
 */
export function ProductsPage() {
  const { isAdministrator } = useSession()
  const { values, page, setFilter, setPage } = useListParams(FILTER_KEYS)
  const { data, error, isPending, isPlaceholderData } = useProducts({
    search: values.search || undefined,
    categoryId: values.category || undefined,
    isActive: toBooleanFilter(values.status),
    page,
  })
  // El filtro incluye también las categorías inactivas: sus productos siguen existiendo.
  const categories = useCategoryOptions({ onlyActive: false })

  const activation = useActivationDialog({
    entityLabel: 'el producto',
    deactivate: deactivateProduct,
    reactivate: reactivateProduct,
    invalidate: [productKeys.all, categoryKeys.all],
    deactivateConsequences: (
      <>
        <p>Dejará de aparecer en el editor de cotizaciones.</p>
        <p>
          Los borradores que lo contienen no se podrán emitir hasta quitarlo, y no se copiará al
          duplicar una cotización. Las cotizaciones emitidas no cambian.
        </p>
      </>
    ),
    reactivateConsequences: <p>Volverá a estar disponible en el editor de cotizaciones.</p>,
  })

  const hasFilters = Boolean(values.search || values.category || values.status)

  return (
    <>
      <PageHeader
        title="Productos"
        description={
          isAdministrator
            ? 'Catálogo de productos y precios vigentes.'
            : 'Catálogo de productos y precios vigentes (solo lectura).'
        }
        actions={
          isAdministrator && (
            <ButtonLink to="/products/new" variant="primary">
              Nuevo producto
            </ButtonLink>
          )
        }
      />

      <div className="toolbar">
        <SearchInput
          label="Buscar productos por nombre"
          placeholder="Buscar por nombre…"
          value={values.search}
          onChange={(value) => setFilter('search', value.trim())}
        />
        <select
          className="input input--select"
          aria-label="Filtrar por categoría"
          value={values.category}
          onChange={(event) => setFilter('category', event.target.value)}
        >
          <option value="">Todas las categorías</option>
          {categories.data?.items.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
              {category.isActive ? '' : ' (inactiva)'}
            </option>
          ))}
        </select>
        <select
          className="input input--select"
          aria-label="Filtrar por estado"
          value={values.status}
          onChange={(event) => setFilter('status', event.target.value)}
        >
          <option value="">Todos los estados</option>
          <option value="true">Activos</option>
          <option value="false">Inactivos</option>
        </select>
      </div>

      {error && <Alert tone="danger">{getErrorMessage(error)}</Alert>}
      {isPending && <Spinner />}

      {data &&
        (data.items.length === 0 ? (
          <EmptyState title="No hay productos">
            <p>
              {hasFilters
                ? 'Ningún producto coincide con la búsqueda o los filtros.'
                : 'El catálogo todavía no tiene productos.'}
            </p>
          </EmptyState>
        ) : (
          <>
            <div className="table-wrapper" aria-busy={isPlaceholderData}>
              <table className="table">
                <thead>
                  <tr>
                    <th scope="col">Producto</th>
                    <th scope="col">Categoría</th>
                    <th scope="col" className="numeric">
                      Precio
                    </th>
                    <th scope="col">Estado</th>
                    {isAdministrator && (
                      <th scope="col" className="actions">
                        <span className="visually-hidden">Acciones</span>
                      </th>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((product) => (
                    <tr key={product.id}>
                      <td>
                        {isAdministrator ? (
                          <Link className="table__primary" to={`/products/${product.id}/edit`}>
                            {product.name}
                          </Link>
                        ) : (
                          <span className="table__primary">{product.name}</span>
                        )}
                        {product.description && (
                          <span className="table__secondary">{product.description}</span>
                        )}
                      </td>
                      <td>
                        {product.categoryName}
                        {!product.categoryIsActive && (
                          <>
                            {' '}
                            <Badge tone="warning">Categoría inactiva</Badge>
                          </>
                        )}
                      </td>
                      <td className="numeric">{formatCurrency(product.price)}</td>
                      <td>
                        <ActiveBadge isActive={product.isActive} />
                      </td>
                      {isAdministrator && (
                        <td className="actions">
                          <ButtonLink to={`/products/${product.id}/edit`} className="button--small">
                            Editar
                          </ButtonLink>
                          <Button
                            className="button--small"
                            variant={product.isActive ? 'ghost' : 'secondary'}
                            onClick={() => activation.request(product)}
                          >
                            {product.isActive ? 'Desactivar' : 'Reactivar'}
                          </Button>
                        </td>
                      )}
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
