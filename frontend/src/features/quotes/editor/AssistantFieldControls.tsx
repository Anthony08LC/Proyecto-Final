import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useId, useState } from 'react'
import { getErrorMessage } from '../../../api/apiError'
import {
  ASSISTANT_TEXT_TYPES,
  suggestQuoteText,
  type AssistantStatus,
  type QuoteTextSuggestion,
} from '../../../api/assistant'
import { QUOTE_TEXT_MAX } from '../../../api/quotes'
import { Button } from '../../../components/ui/Button'
import { Alert } from '../../../components/ui/Feedback'
import type { QuoteTextKey } from '../quoteTexts'
import { assistantKeys } from '../queries'

interface AssistantFieldControlsProps {
  quoteId: string
  field: QuoteTextKey
  label: string
  /** Texto actual del campo en pantalla (puede tener cambios sin guardar). */
  currentValue: string
  /** Falso si el asistente no está disponible o el usuario no puede usarlo en esta cotización. */
  enabled: boolean
  disabledReason: string | null
  /** Guarda el texto aceptado en el borrador; devuelve `true` si se guardó. */
  onAccept: (text: string) => Promise<boolean>
}

/**
 * Botón «Generar con IA» o «Mejorar con IA» de un texto comercial y la sugerencia resultante
 * (sección 11.3). La sugerencia se puede editar; solo se guarda al aceptarla. Mientras espera,
 * el resto del formulario sigue disponible.
 */
export function AssistantFieldControls({
  quoteId,
  field,
  label,
  currentValue,
  enabled,
  disabledReason,
  onAccept,
}: AssistantFieldControlsProps) {
  const queryClient = useQueryClient()
  const panelId = useId()
  const [suggestion, setSuggestion] = useState<QuoteTextSuggestion | null>(null)
  const [draft, setDraft] = useState('')
  const [isAccepting, setIsAccepting] = useState(false)

  const hasText = currentValue.trim().length > 0
  const action = hasText ? 'Improve' : 'Generate'

  const suggest = useMutation({
    mutationFn: () =>
      suggestQuoteText(quoteId, ASSISTANT_TEXT_TYPES[field], action, hasText ? currentValue : null),
    onSuccess: (result) => {
      setSuggestion(result)
      setDraft(result.suggestion)
      queryClient.setQueryData<AssistantStatus>(assistantKeys.status, (status) =>
        status ? { ...status, remainingThisHour: result.remainingThisHour } : status,
      )
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: assistantKeys.status }),
  })

  const discard = () => {
    setSuggestion(null)
    setDraft('')
    suggest.reset()
  }

  const accept = async () => {
    setIsAccepting(true)
    const saved = await onAccept(draft.trim())
    setIsAccepting(false)
    if (saved) {
      discard()
    }
  }

  return (
    <div className="assistant">
      <div className="assistant__bar">
        <Button
          variant="ghost"
          className="assistant__trigger"
          onClick={() => suggest.mutate()}
          isLoading={suggest.isPending}
          disabled={!enabled || suggestion !== null}
          title={!enabled && disabledReason ? disabledReason : undefined}
          aria-controls={suggestion ? panelId : undefined}
        >
          <span aria-hidden="true">✦</span>{' '}
          {suggest.isPending
            ? 'Redactando…'
            : hasText
              ? `Mejorar ${label.toLowerCase()} con IA`
              : `Generar ${label.toLowerCase()} con IA`}
        </Button>
      </div>

      {suggest.isError && (
        <Alert
          tone="danger"
          actions={
            <Button variant="ghost" onClick={() => suggest.reset()}>
              Cerrar
            </Button>
          }
        >
          {getErrorMessage(suggest.error)}
        </Alert>
      )}

      {suggestion && (
        <div
          id={panelId}
          className="assistant__panel"
          role="region"
          aria-label={`Sugerencia de IA para ${label}`}
        >
          <p className="assistant__title">
            Sugerencia de IA · revísela antes de aceptarla; no se guarda hasta que la acepte.
          </p>
          {suggestion.original && (
            <div className="assistant__original">
              <p className="assistant__caption">Texto original</p>
              <p className="assistant__text">{suggestion.original}</p>
            </div>
          )}
          <label className="assistant__caption" htmlFor={`${panelId}-text`}>
            {suggestion.original ? 'Versión mejorada (editable)' : 'Texto propuesto (editable)'}
          </label>
          <textarea
            id={`${panelId}-text`}
            className="input input--textarea assistant__editor"
            value={draft}
            maxLength={QUOTE_TEXT_MAX}
            onChange={(event) => setDraft(event.target.value)}
          />
          <div className="form-actions">
            <Button onClick={discard} disabled={isAccepting}>
              Descartar
            </Button>
            <Button
              variant="primary"
              onClick={accept}
              isLoading={isAccepting}
              disabled={draft.trim().length === 0}
            >
              Aceptar y guardar
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
