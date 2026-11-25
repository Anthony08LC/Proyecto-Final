import type { QuoteStatus } from '../api/types'

/** Nombres en español de los estados de una cotización (spec 8.6). */
export const QUOTE_STATUS_LABELS: Record<QuoteStatus, string> = {
  Draft: 'Borrador',
  Issued: 'Emitida',
  Accepted: 'Aceptada',
  Rejected: 'Rechazada',
  Expired: 'Vencida',
  Cancelled: 'Cancelada',
}

export const QUOTE_STATUS_TONES: Record<
  QuoteStatus,
  'neutral' | 'info' | 'success' | 'danger' | 'warning'
> = {
  Draft: 'neutral',
  Issued: 'info',
  Accepted: 'success',
  Rejected: 'danger',
  Expired: 'warning',
  Cancelled: 'neutral',
}
