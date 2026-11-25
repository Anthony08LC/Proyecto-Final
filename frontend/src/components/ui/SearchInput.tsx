import { useEffect, useState } from 'react'

interface SearchInputProps {
  value: string
  onChange: (value: string) => void
  placeholder: string
  label: string
  /** Espera antes de buscar, para no consultar la API en cada tecla. */
  delayMs?: number
}

/** Búsqueda que se aplica mientras se escribe, con una pequeña espera. */
export function SearchInput({
  value,
  onChange,
  placeholder,
  label,
  delayMs = 300,
}: SearchInputProps) {
  const [text, setText] = useState(value)
  const [syncedValue, setSyncedValue] = useState(value)

  // Si el valor externo cambia (por ejemplo, al volver atrás en el historial), se sincroniza el
  // campo durante el render, sin un efecto adicional.
  if (value !== syncedValue) {
    setSyncedValue(value)
    setText(value)
  }

  useEffect(() => {
    if (text === value) {
      return undefined
    }
    const timer = window.setTimeout(() => onChange(text), delayMs)
    return () => window.clearTimeout(timer)
  }, [text, value, onChange, delayMs])

  return (
    <input
      type="search"
      className="input input--search"
      aria-label={label}
      placeholder={placeholder}
      value={text}
      onChange={(event) => setText(event.target.value)}
    />
  )
}
