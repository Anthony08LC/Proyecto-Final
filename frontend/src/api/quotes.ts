import { request } from './http'
import type { CreatedResponse, PagedResult, QuoteStatus } from './types'

/**
 * Cotizaciones (RF-COT-01 a 14). Todos los montos vienen calculados por el backend: el frontend
 * solo los muestra (RF-COT-06, RNF-DAT-02). Ventas ve todas las cotizaciones, pero solo modifica
 * las suyas (RN-32); `actions` indica qué puede hacer el usuario actual.
 */

/** Cotización en listados (`QuoteSummaryDto`). */
export interface QuoteSummary {
  id: string
  number: string | null
  status: QuoteStatus
  customerId: string
  customerName: string
  ownerId: string
  ownerName: string
  total: number
  /** Verdadero en borradores: el total es estimado hasta emitir. */
  isEstimate: boolean
  createdAt: string
  issuedAt: string | null
  validUntil: string | null
}

/** Desglose financiero calculado por el motor (`QuoteBreakdownDto`). */
export interface QuoteBreakdown {
  subtotal: number
  tierDiscountRate: number
  vipDiscountRate: number
  discountRate: number
  discountAmount: number
  taxableBase: number
  vatRate: number
  vatAmount: number
  total: number
  isEstimate: boolean
}

export interface QuoteLine {
  lineNumber: number
  productId: string
  productName: string
  productDescription: string | null
  categoryName: string
  unitPrice: number
  quantity: number
  lineTotal: number
  /** Solo en borradores: `false` si el producto se desactivó y bloquea la emisión. */
  productIsActive: boolean | null
}

/** Cliente de la cotización; en las emitidas, con los datos congelados al emitir (RN-12). */
export interface QuoteCustomer {
  id: string
  name: string
  company: string | null
  email: string | null
  phone: string | null
  isVip: boolean
  isActive: boolean
}

/** Acciones permitidas según el estado y los permisos del usuario actual (RF-COT-13). */
export interface QuoteActions {
  canEdit: boolean
  canIssue: boolean
  canAccept: boolean
  canReject: boolean
  canCancel: boolean
  canDuplicate: boolean
  canDownloadPdf: boolean
  canUseAssistant: boolean
}

export interface QuoteHistoryEntry {
  occurredAt: string
  userName: string | null
  eventType: string
  detail: string
}

export interface QuoteDetail {
  id: string
  number: string | null
  status: QuoteStatus
  customer: QuoteCustomer
  ownerId: string
  ownerName: string
  sourceQuoteId: string | null
  sourceQuoteNumber: string | null
  introduction: string | null
  executiveSummary: string | null
  notes: string | null
  commercialTerms: string | null
  lines: QuoteLine[]
  breakdown: QuoteBreakdown
  issuedAt: string | null
  validUntil: string | null
  closedAt: string | null
  closeReason: string | null
  createdAt: string
  updatedAt: string
  actions: QuoteActions
  history: QuoteHistoryEntry[]
}

export interface QuoteTexts {
  introduction: string | null
  executiveSummary: string | null
  notes: string | null
  commercialTerms: string | null
}

export interface IssueQuoteResult {
  number: string
  issuedAt: string
  validUntil: string
}

export interface DuplicateQuoteResult {
  quoteId: string
  /** Productos inactivos que no se copiaron (RN-25). */
  omittedProducts: string[]
}

export interface QuoteFilters {
  number?: string | undefined
  customerId?: string | undefined
  status?: QuoteStatus | undefined
  ownerId?: string | undefined
  /** Fechas de creación aaaa-mm-dd, en hora de Costa Rica, ambas inclusive. */
  from?: string | undefined
  to?: string | undefined
  page?: number | undefined
  pageSize?: number | undefined
}

/** Límites del dominio (`Quote`). */
export const QUOTE_TEXT_MAX = 4000
export const QUOTE_REASON_MAX = 500

/** Búsqueda paginada, más recientes primero (RF-COT-14). */
export function getQuotes(filters: QuoteFilters, signal?: AbortSignal) {
  return request<PagedResult<QuoteSummary>>('/api/quotes', { query: { ...filters }, signal })
}

/** Detalle con líneas, textos, desglose, historial y acciones permitidas (RF-COT-13). */
export function getQuote(id: string, signal?: AbortSignal) {
  return request<QuoteDetail>(`/api/quotes/${id}`, { signal })
}

/** Crea un borrador para un cliente activo (RF-COT-01). */
export async function createQuote(customerId: string): Promise<string> {
  const created = await request<CreatedResponse>('/api/quotes', {
    method: 'POST',
    body: { customerId },
  })
  return created.id
}

/** Agrega un producto; si ya está, suma la cantidad (RF-COT-02, RN-28). */
export function addQuoteLine(id: string, productId: string, quantity: number) {
  return request<QuoteBreakdown>(`/api/quotes/${id}/lines`, {
    method: 'POST',
    body: { productId, quantity },
  })
}

/** Cambia la cantidad de una línea (RF-COT-03). */
export function changeQuoteLineQuantity(id: string, productId: string, quantity: number) {
  return request<QuoteBreakdown>(`/api/quotes/${id}/lines/${productId}`, {
    method: 'PUT',
    body: { quantity },
  })
}

/** Quita una línea; las demás se renumeran (RF-COT-03). */
export function removeQuoteLine(id: string, productId: string) {
  return request<QuoteBreakdown>(`/api/quotes/${id}/lines/${productId}`, { method: 'DELETE' })
}

/** Cambia el cliente del borrador; el descuento VIP se recalcula (RF-COT-04). */
export function changeQuoteCustomer(id: string, customerId: string) {
  return request<QuoteBreakdown>(`/api/quotes/${id}/customer`, {
    method: 'PUT',
    body: { customerId },
  })
}

/** Guarda los cuatro textos comerciales (RF-COT-05). */
export function updateQuoteTexts(id: string, texts: QuoteTexts) {
  return request<void>(`/api/quotes/${id}/texts`, { method: 'PUT', body: texts })
}

/** Emite: número consecutivo, vigencia y congelamiento (RF-COT-07). */
export function issueQuote(id: string) {
  return request<IssueQuoteResult>(`/api/quotes/${id}/issue`, { method: 'POST' })
}

/** Registra la aceptación dentro de la vigencia (RF-COT-08). */
export function acceptQuote(id: string) {
  return request<void>(`/api/quotes/${id}/accept`, { method: 'POST' })
}

/** Registra el rechazo con motivo opcional (RF-COT-09). */
export function rejectQuote(id: string, reason: string | null) {
  return request<void>(`/api/quotes/${id}/reject`, { method: 'POST', body: { reason } })
}

/** Cancela; el motivo es obligatorio si estaba emitida (RF-COT-10, RN-34). */
export function cancelQuote(id: string, reason: string | null) {
  return request<void>(`/api/quotes/${id}/cancel`, { method: 'POST', body: { reason } })
}

/** Crea un borrador nuevo a partir de cualquier cotización (RF-COT-12). */
export function duplicateQuote(id: string) {
  return request<DuplicateQuoteResult>(`/api/quotes/${id}/duplicate`, { method: 'POST' })
}

/**
 * Vista previa HTML del documento comercial, en cualquier estado (RF-DOC-01). Los borradores
 * llevan marca de agua (RN-36). Se muestra en un marco aislado, sin scripts.
 */
export function getQuotePreview(id: string, signal?: AbortSignal) {
  return request<string>(`/api/quotes/${id}/preview`, { accept: 'text/html', signal })
}

/** PDF de una cotización emitida (RF-DOC-02, RN-36). */
export function getQuotePdf(id: string) {
  return request<Blob>(`/api/quotes/${id}/pdf`, {
    accept: 'application/pdf',
    responseType: 'blob',
  })
}
