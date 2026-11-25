import { useState } from 'react'
import type { QuoteLine } from '../../../api/quotes'
import { Button } from '../../../components/ui/Button'
import { Badge, EmptyState } from '../../../components/ui/Feedback'
import { formatCurrency } from '../../../lib/format'
import { QUANTITY_FORMAT_MESSAGE, parseQuantity, quantityToInput } from '../../../lib/quantity'

interface EditableLinesTableProps {
  lines: QuoteLine[]
  onChangeQuantity: (line: QuoteLine, quantity: number) => void
  onRemove: (line: QuoteLine) => void
}

/**
 * Líneas del borrador con cantidad editable y botón para quitar (RF-COT-03). Los totales de
 * línea los calcula el backend.
 */
export function EditableLinesTable({ lines, onChangeQuantity, onRemove }: EditableLinesTableProps) {
  if (lines.length === 0) {
    return (
      <EmptyState title="Sin productos">
        <p>Busque un producto arriba para agregarlo. Se necesita al menos uno para emitir.</p>
      </EmptyState>
    )
  }

  return (
    <div className="table-wrapper section-table">
      <table className="table lines-table">
        <thead>
          <tr>
            <th scope="col">#</th>
            <th scope="col">Producto</th>
            <th scope="col" className="numeric">
              Precio unitario
            </th>
            <th scope="col" className="numeric">
              Cantidad
            </th>
            <th scope="col" className="numeric">
              Total
            </th>
            <th scope="col" className="actions">
              <span className="visually-hidden">Acciones</span>
            </th>
          </tr>
        </thead>
        <tbody>
          {lines.map((line) => (
            <tr key={line.productId}>
              <td>{line.lineNumber}</td>
              <td>
                <span className="table__primary">{line.productName}</span>
                <span className="table__secondary">{line.categoryName}</span>
                {line.productIsActive === false && (
                  <Badge tone="danger">Producto inactivo: quítelo para poder emitir</Badge>
                )}
              </td>
              <td className="numeric">{formatCurrency(line.unitPrice)}</td>
              <td className="numeric">
                {/* La clave reinicia el campo cuando la API devuelve otra cantidad. */}
                <QuantityInput
                  key={`${line.productId}-${line.quantity}`}
                  line={line}
                  onCommit={(quantity) => onChangeQuantity(line, quantity)}
                />
              </td>
              <td className="numeric">{formatCurrency(line.lineTotal)}</td>
              <td className="actions">
                <Button
                  variant="ghost"
                  className="button--small"
                  onClick={() => onRemove(line)}
                  aria-label={`Quitar ${line.productName}`}
                >
                  Quitar
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

/**
 * Cantidad editable: se valida mientras se escribe y se guarda al salir del campo o con Enter;
 * Escape descarta el cambio.
 */
function QuantityInput({
  line,
  onCommit,
}: {
  line: QuoteLine
  onCommit: (quantity: number) => void
}) {
  const [text, setText] = useState(quantityToInput(line.quantity))
  const parsed = parseQuantity(text)
  const error = parsed === null ? QUANTITY_FORMAT_MESSAGE : undefined

  const commit = () => {
    if (parsed !== null && parsed !== line.quantity) {
      onCommit(parsed)
    }
  }

  return (
    <div className="quantity-input">
      <input
        className="input"
        inputMode="decimal"
        autoComplete="off"
        aria-label={`Cantidad de ${line.productName}`}
        aria-invalid={error ? true : undefined}
        title={error}
        value={text}
        onChange={(event) => setText(event.target.value)}
        onBlur={commit}
        onKeyDown={(event) => {
          if (event.key === 'Enter') {
            event.preventDefault()
            commit()
          } else if (event.key === 'Escape') {
            setText(quantityToInput(line.quantity))
          }
        }}
      />
      {error && (
        <p className="field__error" role="alert">
          {error}
        </p>
      )}
    </div>
  )
}
