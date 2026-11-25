import type { AuditEntityType } from './dashboard'
import { request } from './http'
import type { PagedResult } from './types'

/** Evento de auditoría (`AuditEventDto`). La fecha viene en UTC. */
export interface AuditEvent {
  id: number
  occurredAt: string
  userId: string | null
  /** Nombre del usuario al momento del evento; nulo en inicios de sesión fallidos. */
  userName: string | null
  eventType: string
  entityType: AuditEntityType
  entityId: string | null
  detail: string
}

export interface AuditEventFilters {
  userId?: string | undefined
  eventType?: string | undefined
  entityType?: AuditEntityType | undefined
  /** Fechas `aaaa-mm-dd` en hora de Costa Rica, ambas inclusivas. */
  from?: string | undefined
  to?: string | undefined
  page?: number | undefined
  pageSize?: number | undefined
}

/** Consulta de auditoría (RF-AUD-02): solo lectura, más recientes primero. Solo Administrador. */
export function getAuditEvents(filters: AuditEventFilters, signal?: AbortSignal) {
  return request<PagedResult<AuditEvent>>('/api/audit-events', { query: { ...filters }, signal })
}
