import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useId, useState, type ReactNode } from 'react'
import { ApiError, getErrorMessage } from '../../../api/apiError'
import {
  QUOTE_REASON_MAX,
  acceptQuote,
  cancelQuote,
  issueQuote,
  rejectQuote,
  type QuoteDetail,
} from '../../../api/quotes'
import { Button } from '../../../components/ui/Button'
import { Alert } from '../../../components/ui/Feedback'
import { useToast } from '../../../components/ui/toastContext'
import { formatCurrency, formatDate } from '../../../lib/format'
import { customerKeys } from '../../customers/queries'
import { quoteKeys } from '../queries'

type Action = 'issue' | 'accept' | 'reject' | 'cancel'

interface ActionConfig {
  label: string
  confirmLabel: string
  tone: 'primary' | 'danger'
  title: string
  /** `required`: el motivo es obligatorio; `optional`: se puede indicar; `none`: no aplica. */
  reason: 'required' | 'optional' | 'none'
  consequences: ReactNode
  run: (reason: string | null) => Promise<unknown>
  success: (result: unknown) => string
}

interface QuoteActionsPanelProps {
  quote: QuoteDetail
  /** Hay textos sin guardar en el editor: emitir los perdería. */
  hasUnsavedTexts?: boolean
  /** Contenido adicional del panel (por ejemplo, duplicar). */
  children?: ReactNode
}

/**
 * Acciones del ciclo de vida según el estado y los permisos que devuelve la API (RF-COT-07 a 10,
 * RF-COT-13). Cada acción pide confirmación en el mismo panel, sin ventanas modales, y explica sus
 * consecuencias (RNF-USA-06). Si la API la rechaza, se muestra la causa exacta.
 */
export function QuoteActionsPanel({
  quote,
  hasUnsavedTexts = false,
  children,
}: QuoteActionsPanelProps) {
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const reasonId = useId()
  const [selected, setSelected] = useState<Action | null>(null)
  const [reason, setReason] = useState('')
  // El error del motivo se muestra cuando el usuario escribe o intenta confirmar, no al abrir.
  const [reasonTouched, setReasonTouched] = useState(false)

  const isIssued = quote.status === 'Issued'
  const configs: Record<Action, ActionConfig> = {
    issue: {
      label: 'Emitir',
      confirmLabel: 'Emitir cotización',
      tone: 'primary',
      title: '¿Emitir la cotización?',
      reason: 'none',
      consequences: (
        <>
          <p>
            Se recalcula por última vez con los precios y parámetros vigentes y se congelan las
            líneas, los datos del cliente, la condición VIP, los descuentos, el IVA y el total
            (estimado ahora en {formatCurrency(quote.breakdown.total)}).
          </p>
          <p>Se asigna el número consecutivo oficial y la vigencia según los días configurados.</p>
          <p>Después no se podrá modificar: cualquier cambio se hace duplicándola.</p>
        </>
      ),
      run: () => issueQuote(quote.id),
      success: (result) => {
        const issued = result as { number: string; validUntil: string }
        return `Cotización emitida como ${issued.number}, vigente hasta el ${formatDate(issued.validUntil)}.`
      },
    },
    accept: {
      label: 'Registrar aceptación',
      confirmLabel: 'Registrar aceptación',
      tone: 'primary',
      title: '¿El cliente aceptó la cotización?',
      reason: 'none',
      consequences: (
        <>
          <p>
            Se registra la aceptación con la fecha de hoy. Solo es posible dentro de la vigencia
            {quote.validUntil ? ` (hasta el ${formatDate(quote.validUntil)})` : ''}.
          </p>
          <p>Es un estado final: la cotización ya no podrá cambiar de estado.</p>
        </>
      ),
      run: () => acceptQuote(quote.id),
      success: () => 'Aceptación registrada.',
    },
    reject: {
      label: 'Registrar rechazo',
      confirmLabel: 'Registrar rechazo',
      tone: 'danger',
      title: '¿El cliente rechazó la cotización?',
      reason: 'optional',
      consequences: (
        <p>
          Se registra el rechazo con la fecha de hoy. Es un estado final: la cotización ya no podrá
          cambiar de estado, aunque podrá duplicarla.
        </p>
      ),
      run: (value) => rejectQuote(quote.id, value),
      success: () => 'Rechazo registrado.',
    },
    cancel: {
      label: 'Cancelar cotización',
      confirmLabel: 'Cancelar cotización',
      tone: 'danger',
      title: isIssued ? '¿Anular la cotización emitida?' : '¿Cancelar el borrador?',
      reason: isIssued ? 'required' : 'optional',
      consequences: isIssued ? (
        <p>
          La cotización {quote.number} queda anulada y conserva su número. Es un estado final y el
          motivo es obligatorio.
        </p>
      ) : (
        <p>
          El borrador se cancela sin recibir número. Es un estado final: ya no podrá editarse,
          aunque podrá duplicarlo.
        </p>
      ),
      run: (value) => cancelQuote(quote.id, value),
      success: () => (isIssued ? 'Cotización anulada.' : 'Borrador cancelado.'),
    },
  }

  const available: Action[] = [
    ...(quote.actions.canIssue ? (['issue'] as const) : []),
    ...(quote.actions.canAccept ? (['accept'] as const) : []),
    ...(quote.actions.canReject ? (['reject'] as const) : []),
    ...(quote.actions.canCancel ? (['cancel'] as const) : []),
  ]

  const mutation = useMutation({
    mutationFn: ({ action, value }: { action: Action; value: string | null }) =>
      configs[action].run(value),
    onSuccess: (result, { action }) => {
      notify(configs[action].success(result))
      setSelected(null)
      setReason('')
    },
    // También tras un error: si falló porque algo cambió (un producto o el cliente se desactivó),
    // la pantalla muestra el estado actual.
    onSettled: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: quoteKeys.all }),
        queryClient.invalidateQueries({ queryKey: customerKeys.all }),
      ]),
  })

  const config = selected ? configs[selected] : null
  const trimmedReason = reason.trim()
  const reasonError =
    config?.reason === 'required' && trimmedReason === ''
      ? 'El motivo es obligatorio para anular una cotización emitida.'
      : trimmedReason.length > QUOTE_REASON_MAX
        ? `El motivo no puede superar los ${QUOTE_REASON_MAX} caracteres.`
        : undefined
  const shownReasonError = reasonTouched ? reasonError : undefined
  const serverReasonError =
    mutation.error instanceof ApiError ? mutation.error.fieldErrors['reason']?.[0] : undefined

  const choose = (action: Action) => {
    mutation.reset()
    setReason('')
    setReasonTouched(false)
    setSelected(action)
  }

  if (available.length === 0 && !children) {
    return null
  }

  return (
    <section className="card actions-panel" aria-labelledby="actions-title">
      <h2 id="actions-title" className="card__title">
        Acciones
      </h2>

      {!config && (
        <div className="actions-panel__buttons">
          {available.map((action) => (
            <Button
              key={action}
              variant={action === 'issue' ? 'primary' : 'secondary'}
              onClick={() => choose(action)}
              disabled={action === 'issue' && hasUnsavedTexts}
            >
              {configs[action].label}
            </Button>
          ))}
          {available.includes('issue') && hasUnsavedTexts && (
            <p className="muted">Guarde o descarte los cambios de los textos antes de emitir.</p>
          )}
          {children}
        </div>
      )}

      {config && selected && (
        <form
          className="inline-confirm"
          noValidate
          onSubmit={(event) => {
            event.preventDefault()
            setReasonTouched(true)
            if (!reasonError) {
              mutation.mutate({
                action: selected,
                value: config.reason === 'none' || trimmedReason === '' ? null : trimmedReason,
              })
            }
          }}
        >
          <p className="inline-confirm__title">{config.title}</p>
          <div className="inline-confirm__body">{config.consequences}</div>

          {config.reason !== 'none' && (
            <div
              className={`field${shownReasonError || serverReasonError ? ' field--invalid' : ''}`}
            >
              <label className="field__label" htmlFor={reasonId}>
                Motivo{config.reason === 'optional' ? ' (opcional)' : ''}
                {config.reason === 'required' && <span className="field__required"> *</span>}
              </label>
              <textarea
                id={reasonId}
                className="input input--textarea"
                value={reason}
                maxLength={QUOTE_REASON_MAX}
                aria-invalid={shownReasonError || serverReasonError ? true : undefined}
                onChange={(event) => {
                  setReason(event.target.value)
                  setReasonTouched(true)
                }}
                autoFocus
              />
              {(shownReasonError ?? serverReasonError) && (
                <p className="field__error" role="alert">
                  {shownReasonError ?? serverReasonError}
                </p>
              )}
            </div>
          )}

          {mutation.error && !serverReasonError && (
            <Alert tone="danger" title={`No se pudo ${config.label.toLowerCase()}`}>
              {getErrorMessage(mutation.error)}
            </Alert>
          )}

          <div className="inline-confirm__actions">
            <Button onClick={() => setSelected(null)} disabled={mutation.isPending}>
              Volver
            </Button>
            <Button
              type="submit"
              variant={config.tone}
              isLoading={mutation.isPending}
              disabled={Boolean(shownReasonError)}
            >
              {config.confirmLabel}
            </Button>
          </div>
        </form>
      )}
    </section>
  )
}
