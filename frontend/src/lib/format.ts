/**
 * Formatos regionales de la interfaz (RNF-USA-03, RNF-DAT-03): colones `₡1.179.720,00`, fechas
 * `dd/mm/aaaa` y horas en la zona America/Costa_Rica.
 *
 * Solo dan formato a valores que vienen de la API: el frontend nunca calcula montos oficiales
 * (RF-COT-06, RNF-DAT-02).
 */

export const COSTA_RICA_TIME_ZONE = 'America/Costa_Rica'

/**
 * Monto en colones con punto de miles y coma decimal: `1179720` → `₡1.179.720,00`.
 * `Intl` con `es-CR` usa espacios como separador de miles, por eso se arma explícitamente.
 */
export function formatCurrency(amount: number): string {
  if (!Number.isFinite(amount)) {
    return '—'
  }

  const [integerPart = '0', decimalPart = '00'] = Math.abs(amount).toFixed(2).split('.')
  const grouped = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, '.')
  const sign = amount < 0 && Number(integerPart + decimalPart) !== 0 ? '-' : ''

  return `${sign}₡${grouped},${decimalPart}`
}

/** Porcentaje con coma decimal y sin ceros innecesarios: `13` → `13 %`, `2.5` → `2,5 %`. */
export function formatPercentage(value: number): string {
  if (!Number.isFinite(value)) {
    return '—'
  }

  const text = value
    .toFixed(2)
    .replace(/\.?0+$/, '')
    .replace('.', ',')
  return `${text} %`
}

/** Cantidad con hasta 2 decimales y coma decimal: `2.25` → `2,25`, `3` → `3`. */
export function formatQuantity(value: number): string {
  if (!Number.isFinite(value)) {
    return '—'
  }

  const [integerPart = '0', decimalPart = ''] = value
    .toFixed(2)
    .replace(/\.?0+$/, '')
    .split('.')
  const grouped = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, '.')
  return decimalPart ? `${grouped},${decimalPart}` : grouped
}

const DATE_ONLY = /^(\d{4})-(\d{2})-(\d{2})$/

/**
 * Fecha `dd/mm/aaaa`.
 * - Una fecha de negocio sin hora (`2026-12-24`, por ejemplo la vigencia) se muestra tal cual.
 * - Una fecha con hora (UTC) se convierte primero a la fecha de Costa Rica.
 */
export function formatDate(value: string | null | undefined): string {
  if (!value) {
    return '—'
  }

  const dateOnly = DATE_ONLY.exec(value)
  if (dateOnly) {
    const [, year, month, day] = dateOnly
    return `${day}/${month}/${year}`
  }

  const parts = costaRicaParts(value)
  return parts ? `${parts.day}/${parts.month}/${parts.year}` : '—'
}

/** Fecha y hora en Costa Rica, formato 24 horas: `24/11/2026 21:05`. */
export function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return '—'
  }

  const parts = costaRicaParts(value)
  return parts ? `${parts.day}/${parts.month}/${parts.year} ${parts.hour}:${parts.minute}` : '—'
}

const costaRicaFormatter = new Intl.DateTimeFormat('en-US', {
  timeZone: COSTA_RICA_TIME_ZONE,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

type DateParts = Record<'year' | 'month' | 'day' | 'hour' | 'minute', string>

function costaRicaParts(value: string): DateParts | null {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return null
  }

  const parts = Object.fromEntries(
    costaRicaFormatter.formatToParts(date).map((part) => [part.type, part.value]),
  )

  return {
    year: parts['year'] ?? '',
    month: parts['month'] ?? '',
    day: parts['day'] ?? '',
    hour: parts['hour'] ?? '',
    minute: parts['minute'] ?? '',
  }
}
