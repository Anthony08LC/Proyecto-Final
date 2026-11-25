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

/** Nombres en español de los eventos del historial de una cotización (spec 10.3). */
const QUOTE_EVENT_LABELS: Record<string, string> = {
  QuoteCreated: 'Creada',
  QuoteDuplicated: 'Duplicada',
  QuoteIssued: 'Emitida',
  QuoteAccepted: 'Aceptada',
  QuoteRejected: 'Rechazada',
  QuoteCancelled: 'Cancelada',
  QuoteExpired: 'Vencida',
  QuoteTextGenerated: 'Texto generado con IA',
}

export function quoteEventLabel(eventType: string): string {
  return QUOTE_EVENT_LABELS[eventType] ?? eventType
}

/** Número para mostrar: los borradores no tienen número hasta emitirse (RN-13). */
export function quoteDisplayNumber(number: string | null): string {
  return number ?? 'Borrador'
}
