import { request } from './http'
import type { QuoteStatus } from './types'

/** Indicadores globales (RF-DSH-01). */
export interface DashboardIndicators {
  activeCustomers: number
  activeProducts: number
  /** Cotizaciones con número: todas menos los borradores. */
  numberedQuotes: number
  /** Emitidas, Aceptadas, Rechazadas y Vencidas; excluye borradores y canceladas. */
  quotedAmount: number
}

/** Cotización reciente (RF-DSH-02). */
export interface RecentQuote {
  id: string
  number: string | null
  customerName: string
  status: QuoteStatus
  total: number
  isEstimate: boolean
  createdAt: string
}

export type AuditEntityType =
  'User' | 'Customer' | 'Category' | 'Product' | 'Quote' | 'SystemSettings' | 'Session'

/** Evento reciente (RF-DSH-03): de todos para el Administrador, solo los propios para Ventas. */
export interface RecentActivity {
  id: number
  occurredAt: string
  userName: string | null
  eventType: string
  entityType: AuditEntityType
  entityId: string | null
  detail: string
}

export interface Dashboard {
  indicators: DashboardIndicators
  recentQuotes: RecentQuote[]
  recentActivity: RecentActivity[]
}

export function getDashboard(signal?: AbortSignal) {
  return request<Dashboard>('/api/dashboard', { signal })
}
