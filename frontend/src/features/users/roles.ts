import type { UserRole } from '../../api/types'

export const USER_ROLES: readonly UserRole[] = ['Administrator', 'Sales']

export const ROLE_LABELS: Record<UserRole, string> = {
  Administrator: 'Administrador',
  Sales: 'Ventas',
}
