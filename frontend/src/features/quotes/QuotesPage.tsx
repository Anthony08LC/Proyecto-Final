import { Link } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import type { QuoteStatus } from '../../api/types'
import { useSession } from '../../auth/useSession'
import { Button, ButtonLink } from '../../components/ui/Button'
import { Alert, Badge, EmptyState, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { Pagination } from '../../components/ui/Pagination'
import { SearchInput } from '../../components/ui/SearchInput'
import { formatCurrency, formatDate } from '../../lib/format'
import { QUOTE_STATUS_LABELS, QUOTE_STATUS_TONES, quoteDisplayNumber } from '../../lib/quoteStatus'
import { useListParams } from '../../lib/useListParams'
import { CustomerPicker } from '../customers/CustomerPicker'
import { useCustomer } from '../customers/queries'
import { useUserOptions } from '../users/queries'
import { useQuotes } from './queries'

const FILTER_KEYS = ['number', 'customer', 'status', 'owner', 'from', 'to'] as const

const STATUSES = Object.keys(QUOTE_STATUS_LABELS) as QuoteStatus[]

/** Valor del filtro de propietario para «solo mis cotizaciones» (Ventas). */
const MINE = 'mine'

/**
 * Historial y búsqueda de cotizaciones (RF-COT-14): filtros por número, cliente, estado,
 * propietario y rango de fechas de creación; más recientes primero. Ventas ve todas (RN-32).
 */
export function QuotesPage() {
  const { user, isAdministrator } = useSession()
  const { values, page, setFilter, setPage, clearFilters } = useListParams(FILTER_KEYS)

  const rangeIsInvalid = Boolean(values.from && values.to && values.from > values.to)
  const ownerId = values.owner === MINE ? user?.id : values.owner || undefined

  const { data, error, isPending, isPlaceholderData } = useQuotes({
    number: values.number.trim() || undefined,
    customerId: values.customer || undefined,
    status: STATUSES.find((status) => status === values.status),
    ownerId,
    from: rangeIsInvalid ? undefined : values.from || undefined,
    to: rangeIsInvalid ? undefined : values.to || undefined,
    page,
  })

  // El filtro por cualquier ejecutivo necesita la lista de usuarios, que es solo del Administrador.
  const owners = useUserOptions({ enabled: isAdministrator })

  const selectedCustomer = useCustomer(values.customer)
  const hasFilters = FILTER_KEYS.some((key) => values[key])

  return (
    <>
      <PageHeader
        title="Cotizaciones"
        description="Historial de cotizaciones, de la más reciente a la más antigua."
        actions={
          <ButtonLink to="/quotes/new" variant="primary">
            Nueva cotización
          </ButtonLink>
        }
      />

      <div className="toolbar">
        <SearchInput
          label="Buscar por número de cotización"
          placeholder="Número (COT-2026-…)"
          value={values.number}
          onChange={(value) => setFilter('number', value)}
        />
        <CustomerPicker
          label="Filtrar por cliente"
          hideLabel
          placeholder="Cliente…"
          onlyActive={false}
          selected={
            values.customer && selectedCustomer.data
              ? { id: selectedCustomer.data.id, name: selectedCustomer.data.name }
              : null
          }
          onSelect={(customer) => setFilter('customer', customer.id)}
          onClear={() => setFilter('customer', '')}
        />
        <select
          className="input input--select"
          aria-label="Filtrar por estado"
          value={values.status}
          onChange={(event) => setFilter('status', event.target.value)}
        >
          <option value="">Todos los estados</option>
          {STATUSES.map((status) => (
            <option key={status} value={status}>
              {QUOTE_STATUS_LABELS[status]}
            </option>
          ))}
        </select>
        <select
          className="input input--select"
          aria-label="Filtrar por propietario"
          value={values.owner}
          onChange={(event) => setFilter('owner', event.target.value)}
        >
          <option value="">Todos los ejecutivos</option>
          {isAdministrator ? (
            owners.data?.items.map((owner) => (
              <option key={owner.id} value={owner.id}>
                {owner.fullName}
                {owner.isActive ? '' : ' (inactivo)'}
              </option>
            ))
          ) : (
            <option value={MINE}>Solo mis cotizaciones</option>
          )}
        </select>
        <label className="toolbar__date">
          <span>Creada desde</span>
          <input
            type="date"
            className="input"
            value={values.from}
            max={values.to || undefined}
            onChange={(event) => setFilter('from', event.target.value)}
          />
        </label>
        <label className="toolbar__date">
          <span>hasta</span>
          <input
            type="date"
            className="input"
            value={values.to}
            min={values.from || undefined}
            onChange={(event) => setFilter('to', event.target.value)}
          />
        </label>
        {hasFilters && (
          <Button variant="ghost" onClick={clearFilters}>
            Limpiar filtros
          </Button>
        )}
      </div>

      {rangeIsInvalid && (
        <Alert tone="warning">
          La fecha inicial no puede ser posterior a la fecha final; no se aplicó el rango.
        </Alert>
      )}
      {error && <Alert tone="danger">{getErrorMessage(error)}</Alert>}
      {isPending && <Spinner />}

      {data &&
        (data.items.length === 0 ? (
          <EmptyState title="No hay cotizaciones">
            <p>
              {hasFilters
                ? 'Ninguna cotización coincide con los filtros.'
                : 'Todavía no hay cotizaciones. Cree la primera.'}
            </p>
          </EmptyState>
        ) : (
          <>
            <div className="table-wrapper" aria-busy={isPlaceholderData}>
              <table className="table">
                <thead>
                  <tr>
                    <th scope="col">Número</th>
                    <th scope="col">Cliente</th>
                    <th scope="col">Estado</th>
                    <th scope="col">Ejecutivo</th>
                    <th scope="col">Fecha</th>
                    <th scope="col">Vigencia</th>
                    <th scope="col" className="numeric">
                      Total
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((quote) => (
                    <tr key={quote.id}>
                      <td>
                        <Link className="table__primary" to={`/quotes/${quote.id}`}>
                          {quoteDisplayNumber(quote.number)}
                        </Link>
                      </td>
                      <td>{quote.customerName}</td>
                      <td>
                        <Badge tone={QUOTE_STATUS_TONES[quote.status]}>
                          {QUOTE_STATUS_LABELS[quote.status]}
                        </Badge>
                      </td>
                      <td>{quote.ownerName}</td>
                      <td>{formatDate(quote.issuedAt ?? quote.createdAt)}</td>
                      <td>{formatDate(quote.validUntil)}</td>
                      <td className="numeric">
                        {formatCurrency(quote.total)}
                        {quote.isEstimate && <span className="table__secondary">Estimado</span>}
                      </td>
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
