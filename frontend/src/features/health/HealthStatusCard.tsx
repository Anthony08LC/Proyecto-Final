import { getErrorMessage } from '../../api/apiError'
import { formatDateTime } from '../../lib/format'
import { useHealth } from './useHealth'

type Tone = 'success' | 'warning' | 'danger' | 'neutral'

/** Estado de la conexión con el backend y la base de datos (criterio de cierre de F1). */
export function HealthStatusCard() {
  const { data, error, isFetching, dataUpdatedAt, errorUpdatedAt, refetch } = useHealth()
  const lastCheckedAt = Math.max(dataUpdatedAt, errorUpdatedAt)

  let tone: Tone = 'neutral'
  let label = 'Verificando…'
  let description = 'Consultando el estado de la API.'

  if (error) {
    tone = 'danger'
    label = 'API no disponible'
    description = getErrorMessage(error)
  } else if (data === 'Healthy') {
    tone = 'success'
    label = 'Operativo'
    description = 'La API y la base de datos responden correctamente.'
  } else if (data) {
    tone = 'warning'
    label = 'Base de datos no disponible'
    description = 'La API responde, pero no puede conectarse con la base de datos.'
  }

  return (
    <section className="card" aria-labelledby="health-title" aria-busy={isFetching}>
      <header className="card__header">
        <h2 id="health-title" className="card__title">
          Estado del sistema
        </h2>
        <span className={`badge badge--${tone}`} role="status">
          {label}
        </span>
      </header>
      <p className="card__text">{description}</p>
      <footer className="card__footer">
        <span className="muted">
          {lastCheckedAt > 0 &&
            `Última verificación: ${formatDateTime(new Date(lastCheckedAt).toISOString())}`}
        </span>
        <button
          type="button"
          className="button button--secondary"
          onClick={() => void refetch()}
          disabled={isFetching}
        >
          {isFetching ? 'Verificando…' : 'Verificar de nuevo'}
        </button>
      </footer>
    </section>
  )
}
