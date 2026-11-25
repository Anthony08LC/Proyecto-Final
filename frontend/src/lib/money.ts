/**
 * Entrada de montos en colones escritos por el usuario (por ejemplo, el precio de un producto).
 * Solo convierte el texto en número para enviarlo; los montos oficiales de una cotización los
 * calcula siempre el backend (RNF-DAT-02).
 *
 * Formato aceptado: dígitos sin separador de miles y, opcionalmente, coma o punto con 1 o 2
 * decimales (`1500000`, `1500000,50`, `1500000.5`). Se limita a 13 dígitos enteros para que el
 * valor viaje exacto en JSON.
 */
const AMOUNT_PATTERN = /^\d{1,13}(?:[.,]\d{1,2})?$/

export const AMOUNT_FORMAT_MESSAGE =
  'Escriba el monto sin separador de miles y con 2 decimales como máximo (por ejemplo, 1500000,50).'

export function isValidAmountText(text: string): boolean {
  return AMOUNT_PATTERN.test(text.trim())
}

/** Convierte el texto en número, o `null` si el formato no es válido. */
export function parseAmount(text: string): number | null {
  const trimmed = text.trim()
  if (!AMOUNT_PATTERN.test(trimmed)) {
    return null
  }
  return Number(trimmed.replace(',', '.'))
}

/** Texto editable a partir de un monto de la API: `1500000.5` → `1500000,50`. */
export function amountToInput(amount: number): string {
  return amount.toFixed(2).replace('.', ',')
}
