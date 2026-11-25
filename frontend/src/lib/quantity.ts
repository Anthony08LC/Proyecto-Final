/**
 * Cantidades de una línea escritas por el usuario (RN-07): mayores que 0 y con 2 decimales como
 * máximo. Coma o punto decimal; hasta 10 dígitos enteros, como `numeric(12,2)` en la base de datos.
 */
const QUANTITY_PATTERN = /^\d{1,10}(?:[.,]\d{1,2})?$/

export const QUANTITY_FORMAT_MESSAGE =
  'La cantidad debe ser mayor que 0, con 2 decimales como máximo.'

/** Convierte el texto en cantidad, o `null` si no es válida. */
export function parseQuantity(text: string): number | null {
  const trimmed = text.trim()
  if (!QUANTITY_PATTERN.test(trimmed)) {
    return null
  }
  const value = Number(trimmed.replace(',', '.'))
  return value > 0 ? value : null
}

/** Texto editable a partir de una cantidad de la API: `2.5` → `2,5`. */
export function quantityToInput(quantity: number): string {
  return String(quantity).replace('.', ',')
}
