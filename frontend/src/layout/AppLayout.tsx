import { NavLink, Outlet } from 'react-router'
import { useSession } from '../auth/useSession'
import { Button } from '../components/ui/Button'
import { useTheme, type ThemePreference } from '../theme/theme'

interface NavItem {
  to: string
  label: string
  adminOnly?: boolean
}

/** Navegación lateral: solo muestra las opciones del rol (RF-AUT-04). */
const NAV_ITEMS: readonly NavItem[] = [
  { to: '/', label: 'Inicio' },
  { to: '/quotes', label: 'Cotizaciones' },
  { to: '/customers', label: 'Clientes' },
  { to: '/products', label: 'Productos' },
  { to: '/categories', label: 'Categorías', adminOnly: true },
  { to: '/users', label: 'Usuarios', adminOnly: true },
  { to: '/audit', label: 'Auditoría', adminOnly: true },
  { to: '/settings', label: 'Configuración', adminOnly: true },
]

const THEME_LABELS: Record<ThemePreference, string> = {
  system: 'Tema: sistema',
  light: 'Tema: claro',
  dark: 'Tema: oscuro',
}

const ROLE_LABELS = { Administrator: 'Administrador', Sales: 'Ventas' } as const

export function AppLayout() {
  const { user, isAdministrator, end } = useSession()
  const { preference, cycle } = useTheme()

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="sidebar__brand">
          QuoteFlow <span className="sidebar__brand-accent">AI</span>
        </div>
        <nav aria-label="Navegación principal">
          <ul className="sidebar__nav">
            {NAV_ITEMS.filter((item) => !item.adminOnly || isAdministrator).map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  end={item.to === '/'}
                  className={({ isActive }) =>
                    `sidebar__link${isActive ? ' sidebar__link--active' : ''}`
                  }
                >
                  {item.label}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>
        <div className="sidebar__footer">
          {user && (
            <div className="sidebar__user">
              <span className="sidebar__user-name">{user.fullName}</span>
              <span className="muted">{ROLE_LABELS[user.role]}</span>
            </div>
          )}
          <Button variant="ghost" onClick={cycle} title="Cambiar entre sistema, claro y oscuro">
            {THEME_LABELS[preference]}
          </Button>
          <Button variant="ghost" onClick={() => end('logout')}>
            Cerrar sesión
          </Button>
        </div>
      </aside>
      <main className="content">
        <Outlet />
      </main>
    </div>
  )
}
