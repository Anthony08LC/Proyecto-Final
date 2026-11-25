import type { QuoteHistoryEntry } from '../../../api/quotes'
import { formatDateTime } from '../../../lib/format'
import { quoteEventLabel } from '../../../lib/quoteStatus'

/** Historial de eventos de la cotización, del más antiguo al más reciente (RF-COT-13). */
export function QuoteHistory({ entries }: { entries: QuoteHistoryEntry[] }) {
  return (
    <section className="card" aria-labelledby="history-title">
      <h2 id="history-title" className="card__title">
        Historial
      </h2>
      {entries.length === 0 ? (
        <p className="muted">Sin eventos registrados.</p>
      ) : (
        <ol className="timeline">
          {entries.map((entry, index) => (
            <li key={`${entry.occurredAt}-${index}`} className="timeline__item">
              <div className="timeline__header">
                <span className="timeline__title">{quoteEventLabel(entry.eventType)}</span>
                <span className="muted">{formatDateTime(entry.occurredAt)}</span>
              </div>
              <p className="timeline__detail">{entry.detail}</p>
              {entry.userName && <p className="muted">{entry.userName}</p>}
            </li>
          ))}
        </ol>
      )}
    </section>
  )
}
