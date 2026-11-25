import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router'
import { z } from 'zod'
import { ApiError, getErrorMessage } from '../../api/apiError'
import { createUser, updateUser, USER_FULL_NAME_MAX, type User } from '../../api/users'
import { useSession } from '../../auth/useSession'
import { Button, ButtonLink } from '../../components/ui/Button'
import { Alert, Spinner } from '../../components/ui/Feedback'
import { SelectField, TextField } from '../../components/ui/Field'
import { PageHeader } from '../../components/ui/PageHeader'
import { useToast } from '../../components/ui/toastContext'
import {
  applyServerErrors,
  EMAIL_MAX_LENGTH,
  EMAIL_PATTERN,
  PASSWORD_MAX_LENGTH,
  passwordPolicy,
  requiredText,
} from '../../lib/forms'
import { userKeys, useUser } from './queries'
import { ROLE_LABELS, USER_ROLES } from './roles'

const requiredEmail = z
  .string()
  .trim()
  .min(1, 'El correo electrónico es obligatorio.')
  .max(
    EMAIL_MAX_LENGTH,
    `El correo electrónico no puede superar los ${EMAIL_MAX_LENGTH} caracteres.`,
  )
  .regex(EMAIL_PATTERN, 'El correo electrónico no tiene un formato válido.')

const baseSchema = z.object({
  fullName: requiredText('El nombre', USER_FULL_NAME_MAX),
  email: requiredEmail,
  role: z.enum(['Administrator', 'Sales'], { message: 'Seleccione un rol.' }),
  temporaryPassword: z.string(),
})

const createSchema = baseSchema.extend({ temporaryPassword: passwordPolicy })

type UserForm = z.infer<typeof baseSchema>

const FIELDS = ['fullName', 'email', 'role', 'temporaryPassword'] as const

/** Alta (RF-USR-02) y edición (RF-USR-03) de un usuario. Solo Administrador. */
export function UserFormPage() {
  const { id } = useParams()

  if (!id) {
    return <UserFormView />
  }

  return <EditUser id={id} />
}

function EditUser({ id }: { id: string }) {
  const { data, error, isPending } = useUser(id)

  if (isPending) {
    return <Spinner />
  }
  if (error) {
    return <Alert tone="danger">{getErrorMessage(error)}</Alert>
  }
  return <UserFormView user={data} />
}

function UserFormView({ user }: { user?: User }) {
  const isEdit = user !== undefined
  const { user: currentUser } = useSession()
  const isSelf = isEdit && user.id === currentUser?.id
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<UserForm>({
    resolver: zodResolver(isEdit ? baseSchema : createSchema),
    mode: 'onChange',
    defaultValues: {
      fullName: user?.fullName ?? '',
      email: user?.email ?? '',
      role: user?.role ?? 'Sales',
      temporaryPassword: '',
    },
  })
  const { errors, isDirty } = form.formState
  const role = useWatch({ control: form.control, name: 'role' })
  const roleChanged = isEdit && role !== user.role

  const save = useMutation({
    mutationFn: async (values: UserForm) => {
      const input = { fullName: values.fullName, email: values.email, role: values.role }
      if (isEdit) {
        await updateUser(user.id, input)
      } else {
        await createUser({ ...input, temporaryPassword: values.temporaryPassword })
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: userKeys.all })
      notify(isEdit ? 'Usuario actualizado.' : 'Usuario creado.')
      navigate('/users')
    },
    onError: (error) => {
      // 409: correo en uso (junto al campo) o último Administrador activo (mensaje general, RN-31).
      if (error instanceof ApiError && error.status === 409) {
        if (/correo/i.test(error.message)) {
          form.setError('email', { type: 'server', message: error.message })
          setFormError(null)
        } else {
          setFormError(error.message)
        }
        return
      }
      setFormError(applyServerErrors(error, form.setError, FIELDS))
    },
  })

  return (
    <>
      <PageHeader
        title={isEdit ? `Editar «${user.fullName}»` : 'Nuevo usuario'}
        back={{ to: '/users', label: 'Usuarios' }}
      />

      <div className="card form-card">
        {formError && <Alert tone="danger">{formError}</Alert>}

        <form
          className="form"
          noValidate
          onSubmit={form.handleSubmit((values) => {
            setFormError(null)
            save.mutate(values)
          })}
        >
          <TextField
            label="Nombre completo"
            required
            autoFocus
            maxLength={USER_FULL_NAME_MAX}
            error={errors.fullName?.message}
            {...form.register('fullName')}
          />
          <TextField
            label="Correo electrónico"
            type="email"
            required
            autoComplete="off"
            maxLength={EMAIL_MAX_LENGTH}
            error={errors.email?.message}
            hint="Único, aunque pertenezca a un usuario inactivo. Se usa para iniciar sesión."
            {...form.register('email')}
          />
          <SelectField
            label="Rol"
            required
            error={errors.role?.message}
            hint={
              isSelf
                ? 'No puede cambiar su propio rol; otro Administrador debe hacerlo.'
                : roleChanged
                  ? 'Al guardar, las sesiones abiertas del usuario se cerrarán.'
                  : 'Ventas cotiza; el Administrador además gestiona catálogo, usuarios y configuración.'
            }
            {...form.register('role')}
          >
            {/* Uno mismo no cambia su rol: el cambio cerraría su propia sesión. */}
            {(isSelf ? [user.role] : USER_ROLES).map((option) => (
              <option key={option} value={option}>
                {ROLE_LABELS[option]}
              </option>
            ))}
          </SelectField>
          {!isEdit && (
            <TextField
              label="Contraseña temporal"
              type="password"
              required
              autoComplete="new-password"
              maxLength={PASSWORD_MAX_LENGTH}
              error={errors.temporaryPassword?.message}
              hint="Mínimo 10 caracteres, con al menos una letra y un número. El usuario deberá cambiarla al iniciar sesión."
              {...form.register('temporaryPassword')}
            />
          )}
          <div className="form-actions">
            <ButtonLink to="/users">Cancelar</ButtonLink>
            <Button
              type="submit"
              variant="primary"
              isLoading={save.isPending}
              disabled={isEdit && !isDirty}
            >
              {isEdit ? 'Guardar cambios' : 'Crear usuario'}
            </Button>
          </div>
        </form>
      </div>
    </>
  )
}
