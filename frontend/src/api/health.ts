import { send } from './http'

/** Estado de la API según `GET /health`, que también verifica la conexión a PostgreSQL. */
export type HealthStatus = 'Healthy' | 'Degraded' | 'Unhealthy'

const KNOWN_STATUSES: readonly HealthStatus[] = ['Healthy', 'Degraded', 'Unhealthy']

/**
 * Consulta el estado del backend. Responde 200 si la base de datos está disponible y 503 si no;
 * ambos son respuestas válidas. Si la API no responde, lanza un `ApiError` de red.
 */
export async function getHealth(signal?: AbortSignal): Promise<HealthStatus> {
  const { data } = await send<unknown>('/health', { signal, acceptStatus: [503] })
  const status = typeof data === 'string' ? data.trim() : ''

  return KNOWN_STATUSES.find((known) => known === status) ?? 'Unhealthy'
}
