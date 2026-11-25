import type { ReactNode } from 'react'

/** Estados de interfaz reutilizables: avisos, insignias, carga, vacío y error. */

type Tone = 'success' | 'warning' | 'danger' | 'neutral' | 'info'

export function Badge({ tone = 'neutral', children }: { tone?: Tone; children: ReactNode }) {
  return <span className={`badge badge--${tone}`}>{children}</span>
}

/** Insignia de estado activo/inactivo, común a usuarios, clientes, categorías y productos. */
export function ActiveBadge({ isActive }: { isActive: boolean }) {
  return <Badge tone={isActive ? 'success' : 'neutral'}>{isActive ? 'Activo' : 'Inactivo'}</Badge>
}

interface AlertProps {
  tone?: Tone
  title?: string
  children: ReactNode
  actions?: ReactNode
}

export function Alert({ tone = 'info', title, children, actions }: AlertProps) {
  return (
    <div className={`alert alert--${tone}`} role={tone === 'danger' ? 'alert' : 'status'}>
      <div className="alert__body">
        {title && <p className="alert__title">{title}</p>}
        <div>{children}</div>
      </div>
      {actions && <div className="alert__actions">{actions}</div>}
    </div>
  )
}

export function Spinner({ label = 'Cargando…' }: { label?: string }) {
  return (
    <div className="loading" role="status">
      <span className="spinner" aria-hidden="true" />
      <span>{label}</span>
    </div>
  )
}

export function EmptyState({ title, children }: { title: string; children?: ReactNode }) {
  return (
    <div className="empty">
      <p className="empty__title">{title}</p>
      {children && <div className="empty__text">{children}</div>}
    </div>
  )
}
