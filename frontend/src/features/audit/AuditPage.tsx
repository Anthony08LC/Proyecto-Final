import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getErrorMessage } from '../../api/apiError'
import { getAuditEvents, type AuditEventFilters } from '../../api/audit'
import type { AuditEntityType } from '../../api/dashboard'
import { Button } from '../../components/ui/Button'
import { Alert, EmptyState, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { Pagination } from '../../components/ui/Pagination'
import { AUDIT_ENTITY_LABELS, AUDIT_EVENT_TYPES, auditEventLabel } from '../../lib/auditEvents'
import { formatDateTime } from '../../lib/format'
import { useListParams } from '../../lib/useListParams'
import { useUserOptions } from '../users/queries'

const FILTER_KEYS = ['user', 'event', 'entity', 'from', 'to'] as const

const ENTITY_TYPES = Object.keys(AUDIT_ENTITY_LABELS) as AuditEntityType[]

/** Tipos de evento ordenados por su nombre en español para el filtro. */
const EVENT_OPTIONS = AUDIT_EVENT_TYPES.map((type) => ({
  type,
  label: auditEventLabel(type),
})).sort((a, b) => a.label.localeCompare(b.label, 'es'))

/**
 * Consulta de auditoría (RF-AUD-02): filtros por usuario, tipo de evento, tipo de entidad y rango
 * de fechas; más recientes primero; fechas en hora de Costa Rica. Solo lectura y solo Administrador.
 */
export function AuditPage() {
  const { values, page, setFilter, setPage, clearFilters } = useListParams(FILTER_KEYS)
  const users = useUserOptions()

  const rangeIsInvalid = Boolean(values.from && values.to && values.from > values.to)

  const filters: AuditEventFilters = {
    userId: values.user || undefined,
    eventType: AUDIT_EVENT_TYPES.find((type) => type === values.event),
    entityType: ENTITY_TYPES.find((type) => type === values.entity),
    from: rangeIsInvalid ? undefined : values.from || undefined,
    to: rangeIsInvalid ? undefined : values.to || undefined,
    page,
  }

  const { data, error, isPending, isPlaceholderData } = useQuery({
    queryKey: ['audit-events', 'list', filters],
    queryFn: ({ signal }) => getAuditEvents(filters, signal),
    placeholderData: keepPreviousData,
  })

  const hasFilters = FILTER_KEYS.some((key) => values[key])

  return (
    <>
      <PageHeader
        title="Auditoría"
        description="Historial de acciones del sistema, de la más reciente a la más antigua. Solo lectura."
      />

      <div className="toolbar">
        <select
          className="input input--select"
          aria-label="Filtrar por usuario"
          value={values.user}
          onChange={(event) => setFilter('user', event.target.value)}
        >
          <option value="">Todos los usuarios</option>
          {users.data?.items.map((user) => (
            <option key={user.id} value={user.id}>
              {user.fullName}
              {user.isActive ? '' : ' (inactivo)'}
            </option>
          ))}
        </select>
        <select
          className="input input--select"
          aria-label="Filtrar por tipo de evento"
          value={values.event}
          onChange={(event) => setFilter('event', event.target.value)}
        >
          <option value="">Todos los eventos</option>
          {EVENT_OPTIONS.map(({ type, label }) => (
            <option key={type} value={type}>
              {label}
            </option>
          ))}
        </select>
        <select
          className="input input--select"
          aria-label="Filtrar por tipo de entidad"
          value={values.entity}
          onChange={(event) => setFilter('entity', event.target.value)}
        >
          <option value="">Todas las entidades</option>
          {ENTITY_TYPES.map((type) => (
            <option key={type} value={type}>
              {AUDIT_ENTITY_LABELS[type]}
            </option>
          ))}
        </select>
        <label className="toolbar__date">
          <span>Desde</span>
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
          La fecha inicial es posterior a la final; el rango de fechas no se aplica.
        </Alert>
      )}
      {error && <Alert tone="danger">{getErrorMessage(error)}</Alert>}
      {isPending && <Spinner />}

      {data &&
        (data.items.length === 0 ? (
          <EmptyState title="No hay eventos">
            <p>
              {hasFilters
                ? 'Ningún evento coincide con los filtros.'
                : 'Todavía no hay eventos registrados.'}
            </p>
          </EmptyState>
        ) : (
          <>
            <div className="table-wrapper" aria-busy={isPlaceholderData}>
              <table className="table">
                <thead>
                  <tr>
                    <th scope="col">Fecha y hora</th>
                    <th scope="col">Usuario</th>
                    <th scope="col">Evento</th>
                    <th scope="col">Entidad</th>
                    <th scope="col">Detalle</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((event) => (
                    <tr key={event.id}>
                      <td className="nowrap">{formatDateTime(event.occurredAt)}</td>
                      <td>{event.userName ?? '—'}</td>
                      <td>{auditEventLabel(event.eventType)}</td>
                      <td>{AUDIT_ENTITY_LABELS[event.entityType]}</td>
                      <td>{event.detail}</td>
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
