import type { QuoteLine } from '../../../api/quotes'
import { EmptyState } from '../../../components/ui/Feedback'
import { formatCurrency, formatQuantity } from '../../../lib/format'

/** Líneas de la cotización en modo de solo lectura (emitidas y cerradas). */
export function QuoteLinesTable({ lines }: { lines: QuoteLine[] }) {
  return (
    <section className="card" aria-labelledby="lines-title">
      <h2 id="lines-title" className="card__title">
        Productos
      </h2>
      {lines.length === 0 ? (
        <EmptyState title="Sin productos" />
      ) : (
        <div className="table-wrapper section-table">
          <table className="table">
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
              </tr>
            </thead>
            <tbody>
              {lines.map((line) => (
                <tr key={line.productId}>
                  <td>{line.lineNumber}</td>
                  <td>
                    <span className="table__primary">{line.productName}</span>
                    <span className="table__secondary">{line.categoryName}</span>
                  </td>
                  <td className="numeric">{formatCurrency(line.unitPrice)}</td>
                  <td className="numeric">{formatQuantity(line.quantity)}</td>
                  <td className="numeric">{formatCurrency(line.lineTotal)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
