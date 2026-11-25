import type { QuoteDetail } from '../../../api/quotes'
import { QUOTE_TEXT_FIELDS } from '../quoteTexts'

/** Textos comerciales en modo de solo lectura; solo se muestran los que tienen contenido. */
export function QuoteTextsView({ quote }: { quote: QuoteDetail }) {
  const filled = QUOTE_TEXT_FIELDS.filter((field) => quote[field.key])

  return (
    <section className="card" aria-labelledby="texts-title">
      <h2 id="texts-title" className="card__title">
        Textos comerciales
      </h2>
      {filled.length === 0 ? (
        <p className="muted">Sin textos comerciales.</p>
      ) : (
        <dl className="details">
          {filled.map((field) => (
            <div key={field.key} className="details__wide">
              <dt>{field.label}</dt>
              <dd>{quote[field.key]}</dd>
            </div>
          ))}
        </dl>
      )}
    </section>
  )
}
