import { zodResolver } from '@hookform/resolvers/zod'
import { useCallback, useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { QUOTE_TEXT_MAX, updateQuoteTexts, type QuoteDetail } from '../../../api/quotes'
import { Button } from '../../../components/ui/Button'
import { Alert } from '../../../components/ui/Feedback'
import { TextAreaField } from '../../../components/ui/Field'
import { useToast } from '../../../components/ui/toastContext'
import { getErrorMessage } from '../../../api/apiError'
import { applyServerErrors, emptyToNull, optionalText } from '../../../lib/forms'
import { useAssistantStatus } from '../queries'
import { QUOTE_TEXT_FIELDS, type QuoteTextKey } from '../quoteTexts'
import { AssistantFieldControls } from './AssistantFieldControls'
import { useDraftChange } from './useDraftChange'

const schema = z.object({
  introduction: optionalText('La introducción', QUOTE_TEXT_MAX),
  executiveSummary: optionalText('El resumen ejecutivo', QUOTE_TEXT_MAX),
  notes: optionalText('Las observaciones', QUOTE_TEXT_MAX),
  commercialTerms: optionalText('Las condiciones comerciales', QUOTE_TEXT_MAX),
})

type TextsForm = z.infer<typeof schema>

const FIELDS = ['introduction', 'executiveSummary', 'notes', 'commercialTerms'] as const

interface QuoteTextsFormProps {
  quote: QuoteDetail
  /** Avisa si hay cambios sin guardar, para no emitir sin ellos. */
  onDirtyChange: (dirty: boolean) => void
}

/** Los cuatro textos comerciales del borrador: opcionales, 4000 caracteres cada uno (RF-COT-05). */
export function QuoteTextsForm({ quote, onDirtyChange }: QuoteTextsFormProps) {
  const { notify } = useToast()
  const change = useDraftChange(quote.id)
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<TextsForm>({
    resolver: zodResolver(schema),
    mode: 'onChange',
    defaultValues: {
      introduction: quote.introduction ?? '',
      executiveSummary: quote.executiveSummary ?? '',
      notes: quote.notes ?? '',
      commercialTerms: quote.commercialTerms ?? '',
    },
  })
  const { errors, isDirty } = form.formState
  const values = form.watch()

  // Asistente de IA (sección 11): solo en borradores que el usuario puede modificar.
  const canUseAssistant = quote.actions.canUseAssistant
  const assistantStatus = useAssistantStatus(canUseAssistant)
  const assistantEnabled = canUseAssistant && assistantStatus.data?.isAvailable === true
  const assistantDisabledReason = !canUseAssistant
    ? 'El asistente solo está disponible en borradores que usted puede modificar.'
    : assistantStatus.data && !assistantStatus.data.isAvailable
      ? (assistantStatus.data.message ?? 'El asistente de redacción no está disponible.')
      : assistantStatus.isError
        ? 'No se pudo consultar la disponibilidad del asistente.'
        : null

  /**
   * Guarda solo el texto aceptado: los demás campos se envían con su valor guardado, así sus
   * cambios sin guardar siguen pendientes en el formulario.
   */
  const acceptSuggestion = useCallback(
    async (key: QuoteTextKey, text: string): Promise<boolean> => {
      setFormError(null)
      const saved = {
        introduction: quote.introduction,
        executiveSummary: quote.executiveSummary,
        notes: quote.notes,
        commercialTerms: quote.commercialTerms,
      }
      try {
        await change.mutateAsync(() =>
          updateQuoteTexts(quote.id, { ...saved, [key]: emptyToNull(text) }),
        )
        form.resetField(key, { defaultValue: text })
        notify('Sugerencia aceptada y guardada.')
        return true
      } catch (error) {
        setFormError(getErrorMessage(error))
        return false
      }
    },
    [change, form, notify, quote],
  )

  useEffect(() => onDirtyChange(isDirty), [isDirty, onDirtyChange])

  const save = (values: TextsForm) => {
    setFormError(null)
    change.mutate(
      () =>
        updateQuoteTexts(quote.id, {
          introduction: emptyToNull(values.introduction),
          executiveSummary: emptyToNull(values.executiveSummary),
          notes: emptyToNull(values.notes),
          commercialTerms: emptyToNull(values.commercialTerms),
        }),
      {
        onSuccess: () => {
          form.reset(values)
          notify('Textos guardados.')
        },
        onError: (error) => setFormError(applyServerErrors(error, form.setError, FIELDS)),
      },
    )
  }

  return (
    <section className="card" aria-labelledby="texts-form-title">
      <div className="card__header">
        <h2 id="texts-form-title" className="card__title">
          Textos comerciales
        </h2>
        {isDirty && <span className="muted">Cambios sin guardar</span>}
      </div>
      {formError && <Alert tone="danger">{formError}</Alert>}
      {canUseAssistant && assistantDisabledReason && (
        <Alert tone="info">{assistantDisabledReason} Puede escribir los textos manualmente.</Alert>
      )}
      <form className="form texts-form" noValidate onSubmit={form.handleSubmit(save)}>
        {QUOTE_TEXT_FIELDS.map((field) => (
          <div key={field.key} className="texts-form__field">
            <TextAreaField
              label={field.label}
              maxLength={QUOTE_TEXT_MAX}
              error={errors[field.key]?.message}
              hint={`Opcional. Máximo ${QUOTE_TEXT_MAX} caracteres.`}
              {...form.register(field.key)}
            />
            {canUseAssistant && (
              <AssistantFieldControls
                quoteId={quote.id}
                field={field.key}
                label={field.label}
                currentValue={values[field.key] ?? ''}
                enabled={assistantEnabled}
                disabledReason={assistantDisabledReason}
                onAccept={(text) => acceptSuggestion(field.key, text)}
              />
            )}
          </div>
        ))}
        <div className="form-actions">
          <Button onClick={() => form.reset()} disabled={!isDirty || change.isPending}>
            Descartar cambios
          </Button>
          <Button type="submit" variant="primary" isLoading={change.isPending} disabled={!isDirty}>
            Guardar textos
          </Button>
        </div>
      </form>
    </section>
  )
}
