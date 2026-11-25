import { Link } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import { useSession } from '../../auth/useSession'
import { ButtonLink } from '../../components/ui/Button'
import { ActiveBadge, Alert, Badge, EmptyState, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { Pagination } from '../../components/ui/Pagination'
import { SearchInput } from '../../components/ui/SearchInput'
import { formatPhone } from '../../lib/format'
import { toBooleanFilter, useListParams } from '../../lib/useListParams'
import { useCustomers } from './queries'

const FILTER_KEYS = ['search', 'status', 'vip'] as const

/**
 * Cartera de clientes (RF-CLI-02): búsqueda por nombre, empresa o correo y filtros por estado y
 * condición VIP. Ventas y Administrador registran y consultan; la edición es del Administrador.
 */
export function CustomersPage() {
  const { isAdministrator } = useSession()
  const { values, page, setFilter, setPage } = useListParams(FILTER_KEYS)
  const { data, error, isPending, isPlaceholderData } = useCustomers({
    search: values.search.trim() || undefined,
    isActive: toBooleanFilter(values.status),
    isVip: toBooleanFilter(values.vip),
    page,
  })

  const hasFilters = Boolean(values.search || values.status || values.vip)

  return (
    <>
      <PageHeader
        title="Clientes"
        description="Cartera de clientes de TechSolutions CR."
        actions={
          <ButtonLink to="/customers/new" variant="primary">
            Nuevo cliente
          </ButtonLink>
        }
      />

      <div className="toolbar">
        <SearchInput
          label="Buscar clientes por nombre, empresa o correo"
          placeholder="Buscar por nombre, empresa o correo…"
          value={values.search}
          onChange={(value) => setFilter('search', value)}
        />
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
        <select
          className="input input--select"
          aria-label="Filtrar por condición VIP"
          value={values.vip}
          onChange={(event) => setFilter('vip', event.target.value)}
        >
          <option value="">VIP y no VIP</option>
          <option value="true">Solo VIP</option>
          <option value="false">Solo no VIP</option>
        </select>
      </div>

      {error && <Alert tone="danger">{getErrorMessage(error)}</Alert>}
      {isPending && <Spinner />}

      {data &&
        (data.items.length === 0 ? (
          <EmptyState title="No hay clientes">
            <p>
              {hasFilters
                ? 'Ningún cliente coincide con la búsqueda o los filtros.'
                : 'Todavía no hay clientes registrados.'}
            </p>
          </EmptyState>
        ) : (
          <>
            <div className="table-wrapper" aria-busy={isPlaceholderData}>
              <table className="table">
                <thead>
                  <tr>
                    <th scope="col">Cliente</th>
                    <th scope="col">Correo</th>
                    <th scope="col">Teléfono</th>
                    <th scope="col">Estado</th>
                    {isAdministrator && (
                      <th scope="col" className="actions">
                        <span className="visually-hidden">Acciones</span>
                      </th>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((customer) => (
                    <tr key={customer.id}>
                      <td>
                        <Link className="table__primary" to={`/customers/${customer.id}`}>
                          {customer.name}
                        </Link>
                        {customer.isVip && (
                          <>
                            {' '}
                            <Badge tone="info">VIP</Badge>
                          </>
                        )}
                        {customer.company && (
                          <span className="table__secondary">{customer.company}</span>
                        )}
                      </td>
                      <td>{customer.email ?? '—'}</td>
                      <td>{formatPhone(customer.phone)}</td>
                      <td>
                        <ActiveBadge isActive={customer.isActive} />
                      </td>
                      {isAdministrator && (
                        <td className="actions">
                          <ButtonLink
                            to={`/customers/${customer.id}/edit`}
                            className="button--small"
                          >
                            Editar
                          </ButtonLink>
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
    </>
  )
}
