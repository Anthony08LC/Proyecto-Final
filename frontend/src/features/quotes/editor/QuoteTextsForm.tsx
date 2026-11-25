import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { QUOTE_TEXT_MAX, updateQuoteTexts, type QuoteDetail } from '../../../api/quotes'
import { Button } from '../../../components/ui/Button'
import { Alert } from '../../../components/ui/Feedback'
import { TextAreaField } from '../../../components/ui/Field'
import { useToast } from '../../../components/ui/toastContext'
import { applyServerErrors, emptyToNull, optionalText } from '../../../lib/forms'
import { QUOTE_TEXT_FIELDS } from '../quoteTexts'
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
      <form className="form texts-form" noValidate onSubmit={form.handleSubmit(save)}>
        {QUOTE_TEXT_FIELDS.map((field) => (
          <TextAreaField
            key={field.key}
            label={field.label}
            maxLength={QUOTE_TEXT_MAX}
            error={errors[field.key]?.message}
            hint={`Opcional. Máximo ${QUOTE_TEXT_MAX} caracteres.`}
            {...form.register(field.key)}
          />
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
