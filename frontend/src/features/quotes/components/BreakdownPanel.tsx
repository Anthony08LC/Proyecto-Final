import type { QuoteBreakdown } from '../../../api/quotes'
import { Badge } from '../../../components/ui/Feedback'
import { formatCurrency, formatPercentage } from '../../../lib/format'

/**
 * Desglose financiero tal como lo calcula el backend (RF-COT-06). El frontend no calcula ningún
 * monto: solo da formato. En los borradores se rotula como «Estimado» (RN-22).
 */
export function BreakdownPanel({
  breakdown,
  isUpdating = false,
}: {
  breakdown: QuoteBreakdown
  isUpdating?: boolean
}) {
  return (
    <section className="card breakdown" aria-labelledby="breakdown-title" aria-busy={isUpdating}>
      <div className="card__header">
        <h2 id="breakdown-title" className="card__title">
          Desglose
        </h2>
        {breakdown.isEstimate ? (
          <Badge tone="warning">Estimado</Badge>
        ) : (
          <Badge tone="success">Oficial</Badge>
        )}
      </div>

      <dl className="breakdown__rows">
        <div className="breakdown__row">
          <dt>Subtotal</dt>
          <dd>{formatCurrency(breakdown.subtotal)}</dd>
        </div>
        <div className="breakdown__row breakdown__row--minor">
          <dt>Descuento por volumen</dt>
          <dd>{formatPercentage(breakdown.tierDiscountRate)}</dd>
        </div>
        <div className="breakdown__row breakdown__row--minor">
          <dt>Descuento VIP</dt>
          <dd>{formatPercentage(breakdown.vipDiscountRate)}</dd>
        </div>
        <div className="breakdown__row">
          <dt>Descuento ({formatPercentage(breakdown.discountRate)})</dt>
          <dd>−{formatCurrency(breakdown.discountAmount)}</dd>
        </div>
        <div className="breakdown__row">
          <dt>Base imponible</dt>
          <dd>{formatCurrency(breakdown.taxableBase)}</dd>
        </div>
        <div className="breakdown__row">
          <dt>IVA ({formatPercentage(breakdown.vatRate)})</dt>
          <dd>{formatCurrency(breakdown.vatAmount)}</dd>
        </div>
        <div className="breakdown__row breakdown__row--total">
          <dt>Total</dt>
          <dd>{formatCurrency(breakdown.total)}</dd>
        </div>
      </dl>

      {breakdown.isEstimate && (
        <p className="muted">
          Montos estimados con los precios, la condición VIP y los parámetros vigentes. Se congelan
          al emitir.
        </p>
      )}
    </section>
  )
}
