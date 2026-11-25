import { useId, useState, type KeyboardEvent, type ReactNode } from 'react'

interface ComboboxProps<T> {
  label: string
  /** Oculta la etiqueta visualmente (por ejemplo, en una barra de filtros). */
  hideLabel?: boolean
  placeholder: string
  inputValue: string
  onInputChange: (value: string) => void
  items: readonly T[]
  isLoading?: boolean
  getKey: (item: T) => string
  renderItem: (item: T) => ReactNode
  onSelect: (item: T) => void
  emptyText: string
  disabled?: boolean
  error?: string | undefined
  hint?: ReactNode
}

/**
 * Campo de búsqueda con lista de resultados desplegable, sin ventanas modales (RNF-USA-05).
 * Accesible por teclado (RNF-USA-08): flechas para moverse, Enter para elegir y Escape para cerrar.
 */
export function Combobox<T>({
  label,
  hideLabel = false,
  placeholder,
  inputValue,
  onInputChange,
  items,
  isLoading = false,
  getKey,
  renderItem,
  onSelect,
  emptyText,
  disabled = false,
  error,
  hint,
}: ComboboxProps<T>) {
  const id = useId()
  const listId = `${id}-list`
  const [open, setOpen] = useState(false)
  const [activeIndex, setActiveIndex] = useState(0)

  const select = (item: T) => {
    onSelect(item)
    setOpen(false)
  }

  const onKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setOpen(true)
      setActiveIndex((index) => Math.min(index + 1, Math.max(items.length - 1, 0)))
    } else if (event.key === 'ArrowUp') {
      event.preventDefault()
      setActiveIndex((index) => Math.max(index - 1, 0))
    } else if (event.key === 'Enter') {
      const item = items[activeIndex]
      if (open && item) {
        event.preventDefault()
        select(item)
      }
    } else if (event.key === 'Escape') {
      setOpen(false)
    }
  }

  const activeItem = items[activeIndex]
  const showList = open && !disabled

  return (
    <div className={`field combobox${error ? ' field--invalid' : ''}`}>
      <label className={hideLabel ? 'visually-hidden' : 'field__label'} htmlFor={id}>
        {label}
      </label>
      <input
        id={id}
        type="search"
        className="input"
        role="combobox"
        aria-expanded={showList}
        aria-controls={listId}
        aria-autocomplete="list"
        aria-activedescendant={
          showList && activeItem ? `${id}-option-${getKey(activeItem)}` : undefined
        }
        aria-invalid={error ? true : undefined}
        placeholder={placeholder}
        value={inputValue}
        disabled={disabled}
        autoComplete="off"
        onChange={(event) => {
          onInputChange(event.target.value)
          setActiveIndex(0)
          setOpen(true)
        }}
        onFocus={() => setOpen(true)}
        // Se cierra después del clic en una opción (mousedown se ejecuta antes que blur).
        onBlur={() => setOpen(false)}
        onKeyDown={onKeyDown}
      />
      {showList && (
        <ul className="combobox__list" id={listId} role="listbox" aria-label={label}>
          {isLoading && items.length === 0 && <li className="combobox__empty">Buscando…</li>}
          {!isLoading && items.length === 0 && <li className="combobox__empty">{emptyText}</li>}
          {items.map((item, index) => (
            <li
              key={getKey(item)}
              id={`${id}-option-${getKey(item)}`}
              role="option"
              aria-selected={index === activeIndex}
              className={`combobox__option${index === activeIndex ? ' combobox__option--active' : ''}`}
              onMouseDown={(event) => {
                event.preventDefault()
                select(item)
              }}
              onMouseEnter={() => setActiveIndex(index)}
            >
              {renderItem(item)}
            </li>
          ))}
        </ul>
      )}
      {error ? (
        <p className="field__error" role="alert">
          {error}
        </p>
      ) : (
        hint && <p className="field__hint">{hint}</p>
      )}
    </div>
  )
}
