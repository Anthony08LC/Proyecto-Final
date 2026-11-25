import { useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import type { UserRole } from '../../api/types'
import { deactivateUser, reactivateUser, type User } from '../../api/users'
import { useSession } from '../../auth/useSession'
import { useActivationDialog } from '../../components/ActivationDialog'
import { Button, ButtonLink } from '../../components/ui/Button'
import { ActiveBadge, Alert, Badge, EmptyState, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { Pagination } from '../../components/ui/Pagination'
import { SearchInput } from '../../components/ui/SearchInput'
import { toBooleanFilter, useListParams } from '../../lib/useListParams'
import { ResetPasswordDialog } from './ResetPasswordDialog'
import { userKeys, useUsers } from './queries'
import { ROLE_LABELS, USER_ROLES } from './roles'

const FILTER_KEYS = ['search', 'role', 'status'] as const

/**
 * Gestión de usuarios (RF-USR-01 a 05): búsqueda por nombre o correo, filtros por rol y estado,
 * y acciones de estado y restablecimiento de contraseña con confirmación. Solo Administrador.
 */
export function UsersPage() {
  const { user: currentUser } = useSession()
  const queryClient = useQueryClient()
  const { values, page, setFilter, setPage } = useListParams(FILTER_KEYS)
  const [resetTarget, setResetTarget] = useState<User | null>(null)

  const { data, error, isPending, isPlaceholderData } = useUsers({
    search: values.search.trim() || undefined,
    role: USER_ROLES.find((role) => role === values.role),
    isActive: toBooleanFilter(values.status),
    page,
  })

  const activation = useActivationDialog({
    entityLabel: 'al usuario',
    deactivate: deactivateUser,
    reactivate: reactivateUser,
    invalidate: [userKeys.all],
    deactivateConsequences: (
      <>
        <p>Sus sesiones abiertas se cerrarán de inmediato y no podrá iniciar sesión.</p>
        <p>Sus cotizaciones y su historial de auditoría se conservan. Puede reactivarlo después.</p>
      </>
    ),
    reactivateConsequences: <p>Podrá volver a iniciar sesión con su contraseña actual.</p>,
  })

  const hasFilters = FILTER_KEYS.some((key) => values[key])

  return (
    <>
      <PageHeader
        title="Usuarios"
        description="Cuentas de acceso al sistema y sus roles."
        actions={
          <ButtonLink to="/users/new" variant="primary">
            Nuevo usuario
          </ButtonLink>
        }
      />

      <div className="toolbar">
        <SearchInput
          label="Buscar usuarios por nombre o correo"
          placeholder="Buscar por nombre o correo…"
          value={values.search}
          onChange={(value) => setFilter('search', value)}
        />
        <select
          className="input input--select"
          aria-label="Filtrar por rol"
          value={values.role}
          onChange={(event) => setFilter('role', event.target.value)}
        >
          <option value="">Todos los roles</option>
          {USER_ROLES.map((role: UserRole) => (
            <option key={role} value={role}>
              {ROLE_LABELS[role]}
            </option>
          ))}
        </select>
        <select
          className="input input--select"
          aria-label="Filtrar por estado"
          value={values.status}
          onChange={(event) => setFilter('status', event.target.value)}
        >
          <option value="">Todos los estados</option>
          <option value="true">Activos</option>
          <option value="false">Inactivos</option>
        </select>
      </div>

      {error && <Alert tone="danger">{getErrorMessage(error)}</Alert>}
      {isPending && <Spinner />}

      {data &&
        (data.items.length === 0 ? (
          <EmptyState title="No hay usuarios">
            <p>
              {hasFilters
                ? 'Ningún usuario coincide con la búsqueda o los filtros.'
                : 'Todavía no hay usuarios registrados.'}
            </p>
          </EmptyState>
        ) : (
          <>
            <div className="table-wrapper" aria-busy={isPlaceholderData}>
              <table className="table">
                <thead>
                  <tr>
                    <th scope="col">Nombre</th>
                    <th scope="col">Rol</th>
                    <th scope="col">Estado</th>
                    <th scope="col" className="actions">
                      <span className="visually-hidden">Acciones</span>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((user) => {
                    const isSelf = user.id === currentUser?.id
                    return (
                      <tr key={user.id}>
                        <td>
                          <Link className="table__primary" to={`/users/${user.id}/edit`}>
                            {user.fullName}
                          </Link>
                          {isSelf && (
                            <>
                              {' '}
                              <Badge tone="info">Usted</Badge>
                            </>
                          )}
                          <span className="table__secondary">{user.email}</span>
                        </td>
                        <td>{ROLE_LABELS[user.role]}</td>
                        <td>
                          <ActiveBadge isActive={user.isActive} />
                          {user.mustChangePassword && (
                            <>
                              {' '}
                              <Badge tone="warning">Contraseña temporal</Badge>
                            </>
                          )}
                        </td>
                        <td className="actions">
                          <ButtonLink to={`/users/${user.id}/edit`} className="button--small">
                            Editar
                          </ButtonLink>
                          {/* Uno mismo cambia su contraseña en su perfil y no puede desactivarse (RN-31). */}
                          {!isSelf && (
                            <>
                              <Button
                                className="button--small"
                                variant="ghost"
                                onClick={() => setResetTarget(user)}
                              >
                                Restablecer contraseña
                              </Button>
                              <Button
                                className="button--small"
                                variant={user.isActive ? 'ghost' : 'secondary'}
                                onClick={() =>
                                  activation.request({
                                    id: user.id,
                                    name: user.fullName,
                                    isActive: user.isActive,
                                  })
                                }
                              >
                                {user.isActive ? 'Desactivar' : 'Reactivar'}
                              </Button>
                            </>
                          )}
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
            <Pagination result={data} onPageChange={setPage} />
          </>
        ))}

      {activation.dialog}
      <ResetPasswordDialog
        user={resetTarget}
        onClose={() => setResetTarget(null)}
        onDone={() => queryClient.invalidateQueries({ queryKey: userKeys.all })}
      />
    </>
  )
}
