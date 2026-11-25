import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useId, useState } from 'react'
import { getProducts, type Product } from '../../../api/products'
import { Combobox } from '../../../components/ui/Combobox'
import { formatCurrency } from '../../../lib/format'
import { QUANTITY_FORMAT_MESSAGE, parseQuantity } from '../../../lib/quantity'
import { useDebouncedValue } from '../../../lib/useDebouncedValue'
import { productKeys } from '../../products/queries'

interface ProductAdderProps {
  onAdd: (product: Product, quantity: number) => void
}

const RESULTS = 8

/**
 * Búsqueda de productos activos mientras se escribe y alta con la cantidad indicada, sin
 * ventanas modales (RF-COT-02, RNF-USA-05). Si el producto ya está en la cotización, el backend
 * suma la cantidad a su línea (RN-28).
 */
export function ProductAdder({ onAdd }: ProductAdderProps) {
  const quantityId = useId()
  const [text, setText] = useState('')
  const [quantity, setQuantity] = useState('1')

  const search = useDebouncedValue(text.trim(), 250)
  const filters = { search: search || undefined, isActive: true, pageSize: RESULTS }
  const { data, isFetching } = useQuery({
    queryKey: productKeys.list(filters),
    queryFn: ({ signal }) => getProducts(filters, signal),
    placeholderData: keepPreviousData,
  })

  const parsedQuantity = parseQuantity(quantity)
  const quantityError = parsedQuantity === null ? QUANTITY_FORMAT_MESSAGE : undefined

  return (
    <div className="product-adder">
      <div className="product-adder__search">
        <Combobox<Product>
          label="Agregar producto"
          placeholder="Escriba para buscar un producto activo…"
          inputValue={text}
          onInputChange={setText}
          items={data?.items ?? []}
          isLoading={isFetching}
          getKey={(product) => product.id}
          renderItem={(product) => (
            <span className="combobox__item">
              <span>
                <span className="combobox__item-title">{product.name}</span>
                <span className="combobox__item-meta">{product.categoryName}</span>
              </span>
              <span className="numeric">{formatCurrency(product.price)}</span>
            </span>
          )}
          onSelect={(product) => {
            if (parsedQuantity === null) {
              return
            }
            onAdd(product, parsedQuantity)
            setText('')
            setQuantity('1')
          }}
          emptyText="No hay productos activos que coincidan."
          hint="Elija un producto de la lista con el ratón o con las flechas y Enter."
        />
      </div>
      <div className={`field product-adder__quantity${quantityError ? ' field--invalid' : ''}`}>
        <label className="field__label" htmlFor={quantityId}>
          Cantidad
        </label>
        <input
          id={quantityId}
          className="input"
          inputMode="decimal"
          autoComplete="off"
          value={quantity}
          aria-invalid={quantityError ? true : undefined}
          onChange={(event) => setQuantity(event.target.value)}
        />
        {quantityError && (
          <p className="field__error" role="alert">
            {quantityError}
          </p>
        )}
      </div>
    </div>
  )
}
