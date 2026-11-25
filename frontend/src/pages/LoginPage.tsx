import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { useLocation, useNavigate } from 'react-router'
import { z } from 'zod'
import { login } from '../api/auth'
import { getErrorMessage } from '../api/apiError'
import { useSession } from '../auth/useSession'
import { Button } from '../components/ui/Button'
import { Alert } from '../components/ui/Feedback'
import { TextField } from '../components/ui/Field'

const schema = z.object({
  email: z.string().trim().min(1, 'El correo electrónico es obligatorio.'),
  password: z.string().min(1, 'La contraseña es obligatoria.'),
})

type LoginForm = z.infer<typeof schema>

/** Inicio de sesión (RF-AUT-01). El mensaje de credenciales inválidas lo define el backend. */
export function LoginPage() {
  const { start, endReason } = useSession()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from ?? '/'

  const form = useForm<LoginForm>({
    resolver: zodResolver(schema),
    mode: 'onTouched',
    defaultValues: { email: '', password: '' },
  })

  const mutation = useMutation({
    mutationFn: (values: LoginForm) => login(values.email, values.password),
    onSuccess: (result) => {
      start({ token: result.token, expiresAt: result.expiresAt, user: result.user })
      navigate(result.user.mustChangePassword ? '/change-password' : from, { replace: true })
    },
  })

  const { errors } = form.formState

  return (
    <div className="auth">
      <div className="auth__card">
        <h1 className="auth__title">
          QuoteFlow <span className="sidebar__brand-accent">AI</span>
        </h1>
        <p className="muted">Inicie sesión con su cuenta de TechSolutions CR.</p>

        {endReason === 'expired' && !mutation.error && (
          <Alert tone="warning">Su sesión expiró o ya no es válida. Inicie sesión de nuevo.</Alert>
        )}
        {mutation.error && <Alert tone="danger">{getErrorMessage(mutation.error)}</Alert>}

        <form
          className="form"
          noValidate
          onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
        >
          <TextField
            label="Correo electrónico"
            type="email"
            autoComplete="username"
            autoFocus
            required
            error={errors.email?.message}
            {...form.register('email')}
          />
          <TextField
            label="Contraseña"
            type="password"
            autoComplete="current-password"
            required
            error={errors.password?.message}
            {...form.register('password')}
          />
          <Button type="submit" variant="primary" isLoading={mutation.isPending}>
            Iniciar sesión
          </Button>
        </form>
      </div>
    </div>
  )
}
