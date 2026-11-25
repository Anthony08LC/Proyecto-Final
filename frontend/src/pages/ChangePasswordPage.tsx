import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router'
import { z } from 'zod'
import { changePassword } from '../api/auth'
import { useSession } from '../auth/useSession'
import { Button } from '../components/ui/Button'
import { Alert } from '../components/ui/Feedback'
import { TextField } from '../components/ui/Field'
import { useToast } from '../components/ui/toastContext'
import { applyServerErrors, passwordPolicy } from '../lib/forms'

const schema = z
  .object({
    currentPassword: z.string().min(1, 'La contraseña actual es obligatoria.'),
    newPassword: passwordPolicy,
    confirmPassword: z.string().min(1, 'Confirme la nueva contraseña.'),
  })
  .refine((values) => values.newPassword !== values.currentPassword, {
    path: ['newPassword'],
    message: 'La nueva contraseña debe ser distinta de la actual.',
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Las contraseñas no coinciden.',
  })

type ChangePasswordForm = z.infer<typeof schema>

/**
 * Cambio de contraseña (RF-AUT-06) y cambio obligatorio de la contraseña temporal (RF-AUT-07).
 * En este incremento solo se llega aquí por el cambio obligatorio; el acceso desde el perfil
 * llega con el módulo de perfil (RF-AUT-05).
 */
export function ChangePasswordPage() {
  const { user, renew, end } = useSession()
  const { notify } = useToast()
  const navigate = useNavigate()
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<ChangePasswordForm>({
    resolver: zodResolver(schema),
    mode: 'onChange',
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
  })

  const mutation = useMutation({
    mutationFn: (values: ChangePasswordForm) =>
      changePassword(values.currentPassword, values.newPassword),
    onSuccess: (token) => {
      // El token anterior quedó invalidado (nuevo sello de seguridad); se reemplaza. Lo único que
      // cambia del perfil es que ya no debe cambiar la contraseña (RF-AUT-07).
      if (user) {
        renew(token, { ...user, mustChangePassword: false })
      }
      notify('Contraseña actualizada.')
      navigate('/', { replace: true })
    },
    onError: (error) => {
      setFormError(applyServerErrors(error, form.setError, ['currentPassword', 'newPassword']))
    },
  })

  const { errors } = form.formState

  return (
    <div className="auth">
      <div className="auth__card">
        <h1 className="auth__title">Cambiar contraseña</h1>
        {user?.mustChangePassword && (
          <Alert tone="warning">
            Su contraseña es temporal. Debe cambiarla antes de continuar.
          </Alert>
        )}
        {formError && <Alert tone="danger">{formError}</Alert>}

        <form
          className="form"
          noValidate
          onSubmit={form.handleSubmit((values) => {
            setFormError(null)
            mutation.mutate(values)
          })}
        >
          <TextField
            label="Contraseña actual"
            type="password"
            autoComplete="current-password"
            required
            autoFocus
            error={errors.currentPassword?.message}
            {...form.register('currentPassword', {
              onChange: () => void form.trigger('newPassword'),
            })}
          />
          <TextField
            label="Nueva contraseña"
            type="password"
            autoComplete="new-password"
            required
            hint="Mínimo 10 caracteres, con al menos una letra y un número."
            error={errors.newPassword?.message}
            {...form.register('newPassword', {
              onChange: () => {
                if (form.getValues('confirmPassword')) {
                  void form.trigger('confirmPassword')
                }
              },
            })}
          />
          <TextField
            label="Confirmar nueva contraseña"
            type="password"
            autoComplete="new-password"
            required
            error={errors.confirmPassword?.message}
            {...form.register('confirmPassword')}
          />
          <Button type="submit" variant="primary" isLoading={mutation.isPending}>
            Cambiar contraseña
          </Button>
          <Button variant="ghost" onClick={() => end('logout')} disabled={mutation.isPending}>
            Cerrar sesión
          </Button>
        </form>
      </div>
    </div>
  )
}
