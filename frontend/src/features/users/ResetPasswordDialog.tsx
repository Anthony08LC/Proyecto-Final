import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiError, getErrorMessage } from '../../api/apiError'
import { resetUserPassword, type User } from '../../api/users'
import { ConfirmDialog } from '../../components/ui/ConfirmDialog'
import { TextField } from '../../components/ui/Field'
import { useToast } from '../../components/ui/toastContext'
import { PASSWORD_MAX_LENGTH, passwordPolicy } from '../../lib/forms'

interface ResetPasswordDialogProps {
  /** Usuario al que se asigna la contraseña temporal; `null` cierra el diálogo. */
  user: User | null
  onClose: () => void
  onDone: () => Promise<void> | void
}

/**
 * Restablecimiento de contraseña con confirmación (RF-USR-05): el Administrador define una
 * contraseña temporal que el usuario deberá cambiar al iniciar sesión.
 */
export function ResetPasswordDialog({ user, onClose, onDone }: ResetPasswordDialogProps) {
  const { notify } = useToast()
  const [password, setPassword] = useState('')
  const [touched, setTouched] = useState(false)

  const mutation = useMutation({
    mutationFn: ({ id, value }: { id: string; value: string }) => resetUserPassword(id, value),
    onSuccess: async () => {
      await onDone()
      notify(`Se asignó una contraseña temporal a «${user?.fullName}».`)
      close()
    },
  })

  const close = () => {
    setPassword('')
    setTouched(false)
    mutation.reset()
    onClose()
  }

  const validation = passwordPolicy.safeParse(password)
  const clientError = validation.success ? undefined : validation.error.issues[0]?.message
  const serverError =
    mutation.error instanceof ApiError
      ? mutation.error.fieldErrors['temporaryPassword']?.[0]
      : undefined

  return (
    <ConfirmDialog
      open={user !== null}
      title={`¿Restablecer la contraseña de «${user?.fullName}»?`}
      confirmLabel="Asignar contraseña temporal"
      tone="danger"
      isLoading={mutation.isPending}
      error={mutation.error && !serverError ? getErrorMessage(mutation.error) : null}
      onConfirm={() => {
        setTouched(true)
        if (user && validation.success) {
          mutation.mutate({ id: user.id, value: password })
        }
      }}
      onCancel={close}
    >
      <p>Sus sesiones abiertas se cerrarán y deberá cambiar la contraseña al iniciar sesión.</p>
      <p>Comuníquele la contraseña temporal por un medio seguro: no se vuelve a mostrar.</p>
      <TextField
        label="Contraseña temporal"
        type="password"
        autoComplete="new-password"
        required
        maxLength={PASSWORD_MAX_LENGTH}
        value={password}
        onChange={(event) => setPassword(event.target.value)}
        onBlur={() => setTouched(true)}
        error={(touched ? clientError : undefined) ?? serverError}
        hint="Mínimo 10 caracteres, con al menos una letra y un número."
      />
    </ConfirmDialog>
  )
}
