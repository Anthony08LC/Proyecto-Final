import { Link } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import type { RecentActivity, RecentQuote } from '../../api/dashboard'
import { useSession } from '../../auth/useSession'
import { ButtonLink } from '../../components/ui/Button'
import { Alert, EmptyState, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { auditEventLabel } from '../../lib/auditEvents'
import { formatCurrency, formatDate, formatDateTime } from '../../lib/format'
import { quoteDisplayNumber } from '../../lib/quoteStatus'
import { HealthStatusCard } from '../health/HealthStatusCard'
import { QuoteStatusBadge } from '../quotes/components/QuoteStatusBadge'
import { useDashboard } from './queries'

/**
 * Página de inicio (RF-DSH-01 a 03): indicadores globales, las 5 cotizaciones más recientes y los
 * últimos 10 eventos (de todos para el Administrador; solo los propios para Ventas). Todos los
 * valores vienen calculados por el backend.
 */
export function DashboardPage() {
  const { user, isAdministrator } = useSession()
  const { data, error, isPending } = useDashboard()

  return (
    <>
      <PageHeader
        title={`Hola, ${user?.fullName ?? ''}`}
        description="Resumen del estado comercial"
        actions={
          <ButtonLink to="/quotes/new" variant="primary">
            Nueva cotización
          </ButtonLink>
        }
      />

      {isPending ? (
        <Spinner />
      ) : error ? (
        <Alert tone="danger">{getErrorMessage(error)}</Alert>
      ) : (
        <>
          <section className="stats" aria-label="Indicadores">
            <Stat label="Clientes activos" value={String(data.indicators.activeCustomers)} />
            <Stat label="Productos activos" value={String(data.indicators.activeProducts)} />
            <Stat
              label="Cotizaciones emitidas"
              value={String(data.indicators.numberedQuotes)}
              hint="Todas las que tienen número (sin borradores)."
            />
            <Stat
              label="Monto total cotizado"
              value={formatCurrency(data.indicators.quotedAmount)}
              hint="Emitidas, aceptadas, rechazadas y vencidas. Sin borradores ni canceladas."
            />
          </section>

          <div className="dashboard">
            <RecentQuotesCard quotes={data.recentQuotes} />
            <RecentActivityCard
              events={data.recentActivity}
              title={isAdministrator ? 'Actividad reciente' : 'Mi actividad reciente'}
            />
          </div>
        </>
      )}

      {isAdministrator && (
        <div className="dashboard__footer">
          <HealthStatusCard />
        </div>
      )}
    </>
  )
}

function Stat({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <div className="card stat">
      <p className="stat__label">{label}</p>
      <p className="stat__value">{value}</p>
      {hint && <p className="stat__hint">{hint}</p>}
    </div>
  )
}

function RecentQuotesCard({ quotes }: { quotes: RecentQuote[] }) {
  return (
    <section className="card" aria-labelledby="recent-quotes-title">
      <div className="card__header">
        <h2 id="recent-quotes-title" className="card__title">
          Cotizaciones recientes
        </h2>
        <Link to="/quotes">Ver todas</Link>
      </div>
      {quotes.length === 0 ? (
        <EmptyState title="Aún no hay cotizaciones." />
      ) : (
        <div className="table-wrapper">
          <table className="table">
            <thead>
              <tr>
                <th scope="col">Número</th>
                <th scope="col">Cliente</th>
                <th scope="col">Estado</th>
                <th scope="col" className="numeric">
                  Total
                </th>
              </tr>
            </thead>
            <tbody>
              {quotes.map((quote) => (
                <tr key={quote.id}>
                  <td>
                    <Link className="table__primary" to={`/quotes/${quote.id}`}>
                      {quoteDisplayNumber(quote.number)}
                    </Link>
                    <span className="table__secondary">{formatDate(quote.createdAt)}</span>
                  </td>
                  <td>{quote.customerName}</td>
                  <td>
                    <QuoteStatusBadge status={quote.status} />
                  </td>
                  <td className="numeric">
                    {formatCurrency(quote.total)}
                    {quote.isEstimate && <span className="table__secondary">Estimado</span>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}

function activityLink(event: RecentActivity): string | null {
  if (!event.entityId) {
    return null
  }
  switch (event.entityType) {
    case 'Quote':
      return `/quotes/${event.entityId}`
    case 'Customer':
      return `/customers/${event.entityId}`
    default:
      return null
  }
}

function RecentActivityCard({ events, title }: { events: RecentActivity[]; title: string }) {
  return (
    <section className="card" aria-labelledby="recent-activity-title">
      <h2 id="recent-activity-title" className="card__title">
        {title}
      </h2>
      {events.length === 0 ? (
        <EmptyState title="Sin eventos registrados." />
      ) : (
        <ol className="timeline">
          {events.map((event) => {
            const link = activityLink(event)
            const label = auditEventLabel(event.eventType)
            return (
              <li key={event.id} className="timeline__item">
                <div className="timeline__header">
                  <span className="timeline__title">
                    {link ? <Link to={link}>{label}</Link> : label}
                  </span>
                  <span className="muted">{formatDateTime(event.occurredAt)}</span>
                </div>
                <p className="timeline__detail">{event.detail}</p>
                {event.userName && <p className="muted">{event.userName}</p>}
              </li>
            )
          })}
        </ol>
      )}
    </section>
  )
}
