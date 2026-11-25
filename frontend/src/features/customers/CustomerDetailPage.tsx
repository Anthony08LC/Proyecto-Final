import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link, useParams } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import {
  deactivateCustomer,
  markCustomerAsVip,
  reactivateCustomer,
  removeCustomerVip,
  type Customer,
} from '../../api/customers'
import { useSession } from '../../auth/useSession'
import { useActivationDialog } from '../../components/ActivationDialog'
import { Button, ButtonLink } from '../../components/ui/Button'
import { ConfirmDialog } from '../../components/ui/ConfirmDialog'
import { ActiveBadge, Alert, Badge, EmptyState, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { Pagination } from '../../components/ui/Pagination'
import { useToast } from '../../components/ui/toastContext'
import { formatCurrency, formatDate, formatDateTime, formatPhone } from '../../lib/format'
import { QUOTE_STATUS_LABELS, QUOTE_STATUS_TONES, quoteDisplayNumber } from '../../lib/quoteStatus'
import { useListParams } from '../../lib/useListParams'
import { customerKeys, useCustomer, useCustomerQuotes } from './queries'

/** Detalle de un cliente con su historial de cotizaciones (RF-CLI-03). */
export function CustomerDetailPage() {
  const { id = '' } = useParams()
  const { data, error, isPending } = useCustomer(id)

  if (isPending) {
    return <Spinner />
  }
  if (error) {
    return (
      <>
        <PageHeader title="Cliente" back={{ to: '/customers', label: 'Clientes' }} />
        <Alert tone="danger">{getErrorMessage(error)}</Alert>
      </>
    )
  }
  return <CustomerDetail customer={data} />
}

function CustomerDetail({ customer }: { customer: Customer }) {
  const { isAdministrator } = useSession()

  const activation = useActivationDialog({
    entityLabel: 'el cliente',
    deactivate: deactivateCustomer,
    reactivate: reactivateCustomer,
    invalidate: [customerKeys.all],
    deactivateConsequences: (
      <>
        <p>No aparecerá al crear cotizaciones y no se podrán duplicar sus cotizaciones.</p>
        <p>
          Sus borradores no se podrán emitir mientras esté inactivo. Su historial sigue visible y
          puede reactivarlo cuando quiera.
        </p>
      </>
    ),
    reactivateConsequences: <p>Volverá a estar disponible para crear cotizaciones.</p>,
  })

  return (
    <>
      <PageHeader
        title={customer.name}
        description={customer.company ?? undefined}
        back={{ to: '/customers', label: 'Clientes' }}
        actions={
          isAdministrator && (
            <>
              <ButtonLink to={`/customers/${customer.id}/edit`}>Editar</ButtonLink>
              <VipAction customer={customer} />
              <Button
                variant={customer.isActive ? 'ghost' : 'secondary'}
                onClick={() => activation.request(customer)}
              >
                {customer.isActive ? 'Desactivar' : 'Reactivar'}
              </Button>
            </>
          )
        }
      />

      {!customer.isActive && (
        <Alert tone="neutral">
          Cliente inactivo: no se pueden crear ni duplicar cotizaciones para él.
        </Alert>
      )}

      <section className="card" aria-labelledby="customer-data">
        <div className="card__header">
          <h2 id="customer-data" className="card__title">
            Datos del cliente
          </h2>
          <div>
            {customer.isVip && <Badge tone="info">VIP</Badge>}
            <ActiveBadge isActive={customer.isActive} />
          </div>
        </div>
        <dl className="details">
          <div>
            <dt>Empresa</dt>
            <dd>{customer.company ?? '—'}</dd>
          </div>
          <div>
            <dt>Correo electrónico</dt>
            <dd>{customer.email ?? '—'}</dd>
          </div>
          <div>
            <dt>Teléfono</dt>
            <dd>{formatPhone(customer.phone)}</dd>
          </div>
          <div>
            <dt>Condición</dt>
            <dd>{customer.isVip ? 'VIP' : 'No VIP'}</dd>
          </div>
          <div>
            <dt>Registrado</dt>
            <dd>{formatDateTime(customer.createdAt)}</dd>
          </div>
          <div>
            <dt>Última modificación</dt>
            <dd>{formatDateTime(customer.updatedAt)}</dd>
          </div>
          <div className="details__wide">
            <dt>Observaciones</dt>
            <dd>{customer.notes ?? '—'}</dd>
          </div>
        </dl>
      </section>

      <CustomerQuotes customerId={customer.id} isActive={customer.isActive} />

      {activation.dialog}
    </>
  )
}

/** Marcar o quitar la condición VIP, con confirmación (RF-CLI-05). Solo Administrador. */
function VipAction({ customer }: { customer: Customer }) {
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const [open, setOpen] = useState(false)

  const mutation = useMutation({
    mutationFn: () =>
      customer.isVip ? removeCustomerVip(customer.id) : markCustomerAsVip(customer.id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: customerKeys.all })
      notify(customer.isVip ? 'Se quitó la condición VIP.' : 'El cliente ahora es VIP.')
      setOpen(false)
    },
  })

  return (
    <>
      <Button
        onClick={() => {
          mutation.reset()
          setOpen(true)
        }}
      >
        {customer.isVip ? 'Quitar VIP' : 'Marcar como VIP'}
      </Button>
      <ConfirmDialog
        open={open}
        title={customer.isVip ? '¿Quitar la condición VIP?' : '¿Marcar como cliente VIP?'}
        confirmLabel={customer.isVip ? 'Quitar VIP' : 'Marcar como VIP'}
        isLoading={mutation.isPending}
        error={mutation.error ? getErrorMessage(mutation.error) : null}
        onConfirm={() => mutation.mutate()}
        onCancel={() => setOpen(false)}
      >
        <p>
          {customer.isVip
            ? `«${customer.name}» dejará de recibir el descuento VIP.`
            : `«${customer.name}» recibirá el descuento VIP configurado.`}
        </p>
        <p>
          El cambio se aplica de inmediato a sus borradores. Sus cotizaciones emitidas no cambian.
        </p>
      </ConfirmDialog>
    </>
  )
}

const NO_FILTERS = [] as const

/** Historial de cotizaciones del cliente: número, estado, total y fecha (RF-CLI-03). */
function CustomerQuotes({ customerId, isActive }: { customerId: string; isActive: boolean }) {
  const { page, setPage } = useListParams(NO_FILTERS)
  const { data, error, isPending, isPlaceholderData } = useCustomerQuotes(customerId, page)

  return (
    <section className="card" aria-labelledby="customer-quotes">
      <div className="card__header">
        <h2 id="customer-quotes" className="card__title">
          Cotizaciones
        </h2>
        {isActive && (
          <ButtonLink to={`/quotes/new?customerId=${customerId}`} variant="primary">
            Nueva cotización
          </ButtonLink>
        )}
      </div>

      {error && <Alert tone="danger">{getErrorMessage(error)}</Alert>}
      {isPending && <Spinner />}

      {data &&
        (data.items.length === 0 ? (
          <EmptyState title="Sin cotizaciones">
            <p>Este cliente todavía no tiene cotizaciones.</p>
          </EmptyState>
        ) : (
          <>
            <div className="table-wrapper section-table" aria-busy={isPlaceholderData}>
              <table className="table">
                <thead>
                  <tr>
                    <th scope="col">Número</th>
                    <th scope="col">Estado</th>
                    <th scope="col">Ejecutivo</th>
                    <th scope="col">Fecha</th>
                    <th scope="col" className="numeric">
                      Total
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((quote) => (
                    <tr key={quote.id}>
                      <td>
                        <Link className="table__primary" to={`/quotes/${quote.id}`}>
                          {quoteDisplayNumber(quote.number)}
                        </Link>
                      </td>
                      <td>
                        <Badge tone={QUOTE_STATUS_TONES[quote.status]}>
                          {QUOTE_STATUS_LABELS[quote.status]}
                        </Badge>
                      </td>
                      <td>{quote.ownerName}</td>
                      <td>{formatDate(quote.issuedAt ?? quote.createdAt)}</td>
                      <td className="numeric">
                        {formatCurrency(quote.total)}
                        {quote.isEstimate && <span className="table__secondary">Estimado</span>}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <Pagination result={data} onPageChange={setPage} />
          </>
        ))}
    </section>
  )
}
