import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { getCustomers, type CustomerSummary } from '../../api/customers'
import { Combobox } from '../../components/ui/Combobox'
import { Badge } from '../../components/ui/Feedback'
import { useDebouncedValue } from '../../lib/useDebouncedValue'
import { customerKeys } from './queries'

export interface SelectedCustomer {
  id: string
  name: string
}

interface CustomerPickerProps {
  label: string
  hideLabel?: boolean
  selected: SelectedCustomer | null
  onSelect: (customer: CustomerSummary) => void
  /** Si se indica, borrar el texto del campo quita el cliente elegido (por ejemplo, en un filtro). */
  onClear?: () => void
  /** Solo clientes activos: los inactivos no se pueden usar en cotizaciones (RN-33). */
  onlyActive: boolean
  placeholder?: string
  disabled?: boolean
  error?: string | undefined
  hint?: string
}

const RESULTS = 8

/** Busca y elige un cliente escribiendo su nombre, empresa o correo. */
export function CustomerPicker({
  label,
  hideLabel,
  selected,
  onSelect,
  onClear,
  onlyActive,
  placeholder = 'Buscar cliente por nombre, empresa o correo…',
  disabled,
  error,
  hint,
}: CustomerPickerProps) {
  const [text, setText] = useState(selected?.name ?? '')
  const [syncedName, setSyncedName] = useState(selected?.name ?? '')

  // Si cambia el cliente elegido desde fuera, el campo muestra su nombre.
  const selectedName = selected?.name ?? ''
  if (selectedName !== syncedName) {
    setSyncedName(selectedName)
    setText(selectedName)
  }

  // Con el nombre elegido en el campo se muestran los primeros clientes, no solo ese.
  const search = useDebouncedValue(text === selectedName ? '' : text.trim(), 300)
  const filters = {
    search: search || undefined,
    isActive: onlyActive ? true : undefined,
    pageSize: RESULTS,
  }
  const { data, isFetching } = useQuery({
    queryKey: customerKeys.list(filters),
    queryFn: ({ signal }) => getCustomers(filters, signal),
    placeholderData: keepPreviousData,
    enabled: !disabled,
  })

  return (
    <Combobox<CustomerSummary>
      label={label}
      hideLabel={hideLabel}
      placeholder={placeholder}
      inputValue={text}
      onInputChange={(value) => {
        setText(value)
        if (value.trim() === '' && selected && onClear) {
          onClear()
        }
      }}
      items={data?.items ?? []}
      isLoading={isFetching}
      getKey={(customer) => customer.id}
      renderItem={(customer) => (
        <span className="combobox__item">
          <span>
            <span className="combobox__item-title">{customer.name}</span>
            {customer.company && <span className="combobox__item-meta">{customer.company}</span>}
          </span>
          <span>
            {customer.isVip && <Badge tone="info">VIP</Badge>}
            {!customer.isActive && <Badge>Inactivo</Badge>}
          </span>
        </span>
      )}
      onSelect={(customer) => {
        setText(customer.name)
        onSelect(customer)
      }}
      emptyText={
        onlyActive ? 'No hay clientes activos que coincidan.' : 'No hay clientes que coincidan.'
      }
      disabled={disabled}
      error={error}
      hint={hint}
    />
  )
}
