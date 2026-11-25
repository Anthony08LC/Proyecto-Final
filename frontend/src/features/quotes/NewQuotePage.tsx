import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import { createQuote } from '../../api/quotes'
import { Button, ButtonLink } from '../../components/ui/Button'
import { Alert, Badge } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { useToast } from '../../components/ui/toastContext'
import { CustomerPicker, type SelectedCustomer } from '../customers/CustomerPicker'
import { useCustomer } from '../customers/queries'
import { quoteKeys } from './queries'

/**
 * Crea un borrador (RF-COT-01): exige un cliente activo (RN-33). El propietario es quien lo crea
 * y las condiciones comerciales se llenan con el texto por defecto de la configuración.
 * Admite `?customerId=` para iniciarla desde el detalle de un cliente (RF-CLI-03).
 */
export function NewQuotePage() {
  const [params] = useSearchParams()
  const presetId = params.get('customerId') ?? ''
  const preset = useCustomer(presetId)

  const [picked, setPicked] = useState<(SelectedCustomer & { isVip: boolean }) | null>(null)
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { notify } = useToast()

  // El cliente de la URL solo vale si está activo; el usuario puede elegir otro.
  const presetCustomer =
    preset.data?.isActive === true
      ? { id: preset.data.id, name: preset.data.name, isVip: preset.data.isVip }
      : null
  const customer = picked ?? presetCustomer

  const create = useMutation({
    mutationFn: (customerId: string) => createQuote(customerId),
    onSuccess: async (id) => {
      await queryClient.invalidateQueries({ queryKey: quoteKeys.all })
      notify('Borrador creado.')
      navigate(`/quotes/${id}`, { replace: true })
    },
  })

  return (
    <>
      <PageHeader title="Nueva cotización" back={{ to: '/quotes', label: 'Cotizaciones' }} />

      <div className="card form-card">
        {preset.data && !preset.data.isActive && (
          <Alert tone="warning">
            El cliente «{preset.data.name}» está inactivo y no se pueden crear cotizaciones para él.
            Elija otro cliente.
          </Alert>
        )}
        {create.error && <Alert tone="danger">{getErrorMessage(create.error)}</Alert>}

        <form
          className="form"
          noValidate
          onSubmit={(event) => {
            event.preventDefault()
            if (customer) {
              create.mutate(customer.id)
            }
          }}
        >
          <CustomerPicker
            label="Cliente"
            onlyActive
            selected={customer}
            onSelect={(selected) =>
              setPicked({ id: selected.id, name: selected.name, isVip: selected.isVip })
            }
            hint="Solo clientes activos. Podrá cambiarlo mientras la cotización sea un borrador."
          />
          {customer?.isVip && (
            <p className="muted">
              <Badge tone="info">VIP</Badge> Se aplicará el descuento VIP vigente.
            </p>
          )}
          <p className="muted">
            Se crea como borrador, sin número ni vigencia. Después podrá agregar productos y textos;
            los montos son estimados hasta emitirla.
          </p>
          <div className="form-actions">
            <ButtonLink to="/quotes">Cancelar</ButtonLink>
            <Button
              type="submit"
              variant="primary"
              disabled={!customer}
              isLoading={create.isPending}
            >
              Crear borrador
            </Button>
          </div>
        </form>
      </div>
    </>
  )
}
