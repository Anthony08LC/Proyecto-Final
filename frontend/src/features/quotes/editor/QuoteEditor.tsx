import type { ReactNode } from 'react'
import { getErrorMessage } from '../../../api/apiError'
import {
  addQuoteLine,
  changeQuoteCustomer,
  changeQuoteLineQuantity,
  removeQuoteLine,
  type QuoteDetail,
} from '../../../api/quotes'
import { Alert } from '../../../components/ui/Feedback'
import { CustomerPicker } from '../../customers/CustomerPicker'
import { BreakdownPanel } from '../components/BreakdownPanel'
import { QuoteHistory } from '../components/QuoteHistory'
import { EditableLinesTable } from './EditableLinesTable'
import { ProductAdder } from './ProductAdder'
import { QuoteTextsForm } from './QuoteTextsForm'
import { useDraftChange, useIsDraftChanging } from './useDraftChange'

interface QuoteEditorProps {
  quote: QuoteDetail
  /** Panel de acciones del ciclo de vida, en la columna lateral. */
  actions: ReactNode
  onTextsDirtyChange: (dirty: boolean) => void
}

/**
 * Editor de borrador sin ventanas modales (RNF-USA-05): cliente, productos con búsqueda al
 * escribir, líneas con cantidad editable, textos comerciales y desglose estimado que se
 * actualiza tras cada cambio (RF-COT-02 a 06). Cada cambio se guarda de inmediato en el servidor.
 */
export function QuoteEditor({ quote, actions, onTextsDirtyChange }: QuoteEditorProps) {
  const change = useDraftChange(quote.id)
  const isChanging = useIsDraftChanging(quote.id)

  return (
    <div className="quote-layout">
      <div className="quote-layout__main">
        {change.error && (
          <Alert tone="danger" title="No se pudo aplicar el cambio">
            {getErrorMessage(change.error)}
          </Alert>
        )}

        <section className="card" aria-labelledby="editor-customer">
          <h2 id="editor-customer" className="card__title">
            Cliente
          </h2>
          <div className="editor-section">
            <CustomerPicker
              label="Cliente de la cotización"
              hideLabel
              onlyActive
              selected={{ id: quote.customer.id, name: quote.customer.name }}
              onSelect={(customer) => {
                if (customer.id !== quote.customer.id) {
                  change.mutate(() => changeQuoteCustomer(quote.id, customer.id))
                }
              }}
              hint="Al cambiar de cliente se recalcula el descuento VIP."
            />
            {!quote.customer.isActive && (
              <Alert tone="warning">
                El cliente «{quote.customer.name}» está inactivo: elija otro para poder emitir.
              </Alert>
            )}
          </div>
        </section>

        <section className="card" aria-labelledby="editor-lines">
          <h2 id="editor-lines" className="card__title">
            Productos
          </h2>
          <div className="editor-section">
            <ProductAdder
              onAdd={(product, quantity) =>
                change.mutate(() => addQuoteLine(quote.id, product.id, quantity))
              }
            />
            <EditableLinesTable
              lines={quote.lines}
              onChangeQuantity={(line, quantity) =>
                change.mutate(() => changeQuoteLineQuantity(quote.id, line.productId, quantity))
              }
              onRemove={(line) => change.mutate(() => removeQuoteLine(quote.id, line.productId))}
            />
          </div>
        </section>

        <QuoteTextsForm quote={quote} onDirtyChange={onTextsDirtyChange} />
        <QuoteHistory entries={quote.history} />
      </div>

      <aside className="quote-layout__side">
        <BreakdownPanel breakdown={quote.breakdown} isUpdating={isChanging} />
        {actions}
      </aside>
    </div>
  )
}
