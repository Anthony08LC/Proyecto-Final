import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router'
import { z } from 'zod'
import { getErrorMessage } from '../../api/apiError'
import {
  CUSTOMER_COMPANY_MAX,
  CUSTOMER_NAME_MAX,
  CUSTOMER_NOTES_MAX,
  createCustomer,
  updateCustomer,
  type Customer,
} from '../../api/customers'
import { useSession } from '../../auth/useSession'
import { Button, ButtonLink } from '../../components/ui/Button'
import { Alert, Spinner } from '../../components/ui/Feedback'
import { CheckboxField, TextAreaField, TextField } from '../../components/ui/Field'
import { PageHeader } from '../../components/ui/PageHeader'
import { useToast } from '../../components/ui/toastContext'
import { formatPhone } from '../../lib/format'
import {
  applyServerErrors,
  emptyToNull,
  optionalEmail,
  optionalPhone,
  optionalText,
  requiredText,
} from '../../lib/forms'
import { useDebouncedValue } from '../../lib/useDebouncedValue'
import { customerKeys, useCustomer, useCustomerWithEmail } from './queries'

const schema = z.object({
  name: requiredText('El nombre', CUSTOMER_NAME_MAX),
  company: optionalText('La empresa', CUSTOMER_COMPANY_MAX),
  email: optionalEmail,
  phone: optionalPhone,
  notes: optionalText('Las observaciones', CUSTOMER_NOTES_MAX),
  isVip: z.boolean(),
})

type CustomerForm = z.infer<typeof schema>

const FIELDS = ['name', 'company', 'email', 'phone', 'notes'] as const

/**
 * Registro (RF-CLI-01, Ventas y Administrador) y edición (RF-CLI-04, solo Administrador) de un
 * cliente. La condición VIP solo la asigna el Administrador (RN-21); en la edición se cambia
 * desde el detalle (RF-CLI-05).
 */
export function CustomerFormPage() {
  const { id } = useParams()
  return id ? <EditCustomer id={id} /> : <CustomerFormView />
}

function EditCustomer({ id }: { id: string }) {
  const { data, error, isPending } = useCustomer(id)

  if (isPending) {
    return <Spinner />
  }
  if (error) {
    return <Alert tone="danger">{getErrorMessage(error)}</Alert>
  }
  return <CustomerFormView customer={data} />
}

function CustomerFormView({ customer }: { customer?: Customer }) {
  const isEdit = customer !== undefined
  const { isAdministrator } = useSession()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<CustomerForm>({
    resolver: zodResolver(schema),
    mode: 'onChange',
    defaultValues: {
      name: customer?.name ?? '',
      company: customer?.company ?? '',
      email: customer?.email ?? '',
      phone: customer?.phone ? formatPhone(customer.phone) : '',
      notes: customer?.notes ?? '',
      isVip: false,
    },
  })
  const { errors, isDirty } = form.formState

  // Aviso de posible duplicado mientras se escribe el correo (RF-CLI-01).
  const email = useWatch({ control: form.control, name: 'email' })
  const debouncedEmail = useDebouncedValue(errors.email ? '' : email)
  const { data: duplicate } = useCustomerWithEmail(debouncedEmail, customer?.id)

  const save = useMutation({
    mutationFn: (values: CustomerForm) => {
      const input = {
        name: values.name,
        company: emptyToNull(values.company),
        email: emptyToNull(values.email),
        phone: emptyToNull(values.phone),
        notes: emptyToNull(values.notes),
      }
      return isEdit
        ? updateCustomer(customer.id, input)
        : createCustomer({ ...input, isVip: isAdministrator && values.isVip })
    },
    onSuccess: async (result) => {
      await queryClient.invalidateQueries({ queryKey: customerKeys.all })
      notify(isEdit ? 'Cliente actualizado.' : 'Cliente registrado.')
      if (result.possibleDuplicateEmail) {
        notify(
          'Otro cliente ya usa este correo. Revise si se trata de un registro duplicado.',
          'warning',
        )
      }
      navigate(`/customers/${result.customerId}`)
    },
    onError: (error) => setFormError(applyServerErrors(error, form.setError, FIELDS)),
  })

  const backTo = isEdit ? `/customers/${customer.id}` : '/customers'

  return (
    <>
      <PageHeader
        title={isEdit ? `Editar «${customer.name}»` : 'Nuevo cliente'}
        back={{ to: backTo, label: isEdit ? customer.name : 'Clientes' }}
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
          <div className="form-grid">
            <TextField
              label="Nombre"
              required
              autoFocus
              maxLength={CUSTOMER_NAME_MAX}
              error={errors.name?.message}
              {...form.register('name')}
            />
            <TextField
              label="Empresa"
              maxLength={CUSTOMER_COMPANY_MAX}
              error={errors.company?.message}
              hint="Opcional."
              {...form.register('company')}
            />
            <TextField
              label="Correo electrónico"
              type="email"
              autoComplete="off"
              error={errors.email?.message}
              hint="Opcional."
              {...form.register('email')}
            />
            <TextField
              label="Teléfono"
              type="tel"
              autoComplete="off"
              error={errors.phone?.message}
              hint="Opcional. 8 dígitos, con +506 opcional (por ejemplo, 8888-7777)."
              {...form.register('phone')}
            />
            {duplicate && (
              <div className="form-grid__wide">
                <Alert tone="warning" title="Posible cliente duplicado">
                  El cliente <Link to={`/customers/${duplicate.id}`}>{duplicate.name}</Link> ya usa
                  este correo. Puede continuar si se trata de otro cliente.
                </Alert>
              </div>
            )}
            <div className="form-grid__wide">
              <TextAreaField
                label="Observaciones"
                maxLength={CUSTOMER_NOTES_MAX}
                error={errors.notes?.message}
                hint={`Opcional. Máximo ${CUSTOMER_NOTES_MAX} caracteres.`}
                {...form.register('notes')}
              />
            </div>
            {isAdministrator && !isEdit && (
              <div className="form-grid__wide">
                <CheckboxField
                  label="Cliente VIP"
                  hint="Recibe el descuento VIP en sus cotizaciones. Solo el Administrador puede asignarlo."
                  {...form.register('isVip')}
                />
              </div>
            )}
          </div>
          {isEdit && (
            <p className="muted">
              Los cambios no afectan a las cotizaciones ya emitidas. La condición VIP se cambia
              desde el detalle del cliente.
            </p>
          )}
          <div className="form-actions">
            <ButtonLink to={backTo}>Cancelar</ButtonLink>
            <Button
              type="submit"
              variant="primary"
              isLoading={save.isPending}
              disabled={isEdit && !isDirty}
            >
              {isEdit ? 'Guardar cambios' : 'Registrar cliente'}
            </Button>
          </div>
        </form>
      </div>
    </>
  )
}
