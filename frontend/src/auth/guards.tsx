import { Navigate, Outlet, useLocation } from 'react-router'
import { useSession } from './useSession'

/**
 * Rutas protegidas (RF-AUT-04). La interfaz oculta lo no permitido, pero la autoridad es
 * siempre el backend.
 */

/** Exige sesión. Con contraseña temporal, solo deja pasar a la pantalla de cambio (RF-AUT-07). */
export function RequireAuth() {
  const { user } = useSession()
  const location = useLocation()

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
  }

  if (user.mustChangePassword && location.pathname !== '/change-password') {
    return <Navigate to="/change-password" replace />
  }

  return <Outlet />
}

/** Exige el rol Administrador; Ventas ve la página de acceso denegado. */
export function RequireAdministrator() {
  const { isAdministrator } = useSession()
  return isAdministrator ? <Outlet /> : <Navigate to="/forbidden" replace />
}

/** Para el inicio de sesión: si ya hay sesión, va al inicio. */
export function RedirectIfAuthenticated() {
  const { user } = useSession()
  return user ? <Navigate to="/" replace /> : <Outlet />
}
