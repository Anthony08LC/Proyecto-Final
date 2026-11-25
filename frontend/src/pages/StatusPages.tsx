import { ButtonLink } from '../components/ui/Button'
import { EmptyState } from '../components/ui/Feedback'

export function ForbiddenPage() {
  return (
    <EmptyState title="Acceso denegado">
      <p>Esta sección solo está disponible para el rol Administrador.</p>
      <ButtonLink to="/">Volver al inicio</ButtonLink>
    </EmptyState>
  )
}

export function NotFoundPage() {
  return (
    <EmptyState title="Página no encontrada">
      <p>La dirección no existe o ya no está disponible.</p>
      <ButtonLink to="/">Volver al inicio</ButtonLink>
    </EmptyState>
  )
}
