import { Link } from 'react-router'
import type { QuoteDetail } from '../../../api/quotes'
import { Badge } from '../../../components/ui/Feedback'
import { formatDate, formatDateTime, formatPhone } from '../../../lib/format'
import { quoteDisplayNumber } from '../../../lib/quoteStatus'

/**
 * Encabezado de la cotización: cliente, propietario, fechas, cierre y origen (RF-COT-13).
 * En las emitidas, los datos del cliente son los congelados al emitir (RN-12).
 */
export function QuoteSummaryCard({ quote }: { quote: QuoteDetail }) {
  const { customer } = quote
  const isDraft = quote.status === 'Draft'

  return (
    <section className="card" aria-labelledby="summary-title">
      <h2 id="summary-title" className="card__title">
        {isDraft ? 'Datos del borrador' : 'Datos de la cotización'}
      </h2>
      <dl className="details">
        <div>
          <dt>Cliente</dt>
          <dd>
            <Link to={`/customers/${customer.id}`}>{customer.name}</Link>
            {customer.isVip && (
              <>
                {' '}
                <Badge tone="info">VIP</Badge>
              </>
            )}
            {isDraft && !customer.isActive && (
              <>
                {' '}
                <Badge tone="warning">Inactivo</Badge>
              </>
            )}
          </dd>
        </div>
        <div>
          <dt>Empresa</dt>
          <dd>{customer.company ?? '—'}</dd>
        </div>
        <div>
          <dt>Contacto</dt>
          <dd>
            {customer.email ?? '—'}
            {customer.phone && (
              <span className="table__secondary">{formatPhone(customer.phone)}</span>
            )}
          </dd>
        </div>
        <div>
          <dt>Ejecutivo</dt>
          <dd>{quote.ownerName}</dd>
        </div>
        <div>
          <dt>Creada</dt>
          <dd>{formatDateTime(quote.createdAt)}</dd>
        </div>
        {quote.issuedAt && (
          <div>
            <dt>Emitida</dt>
            <dd>{formatDateTime(quote.issuedAt)}</dd>
          </div>
        )}
        {quote.validUntil && (
          <div>
            <dt>Vigente hasta</dt>
            <dd>{formatDate(quote.validUntil)}</dd>
          </div>
        )}
        {quote.closedAt && (
          <div>
            <dt>Cerrada</dt>
            <dd>{formatDateTime(quote.closedAt)}</dd>
          </div>
        )}
        {quote.sourceQuoteId && (
          <div>
            <dt>Copia de</dt>
            <dd>
              <Link to={`/quotes/${quote.sourceQuoteId}`}>
                {quoteDisplayNumber(quote.sourceQuoteNumber)}
              </Link>
            </dd>
          </div>
        )}
        {quote.closeReason && (
          <div className="details__wide">
            <dt>Motivo</dt>
            <dd>{quote.closeReason}</dd>
          </div>
        )}
      </dl>
    </section>
  )
}
