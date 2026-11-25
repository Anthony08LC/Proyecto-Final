import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { getErrorMessage } from '../../api/apiError'
import {
  COMPANY_ADDRESS_MAX,
  COMPANY_LEGAL_ID_MAX,
  COMPANY_NAME_MAX,
  DEFAULT_TERMS_MAX,
  getCommercialParametersImpact,
  getSettings,
  updateCommercialParameters,
  updateCompanySettings,
  type CommercialParametersInput,
  type SystemSettings,
} from '../../api/settings'
import { Button } from '../../components/ui/Button'
import { ConfirmDialog } from '../../components/ui/ConfirmDialog'
import { Alert, Spinner } from '../../components/ui/Feedback'
import { TextAreaField, TextField } from '../../components/ui/Field'
import { PageHeader } from '../../components/ui/PageHeader'
import { useToast } from '../../components/ui/toastContext'
import {
  commercialPolicyErrors,
  parsePercentage,
  parseValidityDays,
  PERCENTAGE_FORMAT_MESSAGE,
  percentageToInput,
  VALIDITY_DAYS_MESSAGE,
} from '../../lib/commercialParameters'
import { formatCurrency, formatDateTime, formatPercentage, formatPhone } from '../../lib/format'
import {
  applyServerErrors,
  emptyToNull,
  optionalEmail,
  optionalPhone,
  optionalText,
  requiredText,
} from '../../lib/forms'
import {
  AMOUNT_FORMAT_MESSAGE,
  amountToInput,
  isValidAmountText,
  parseAmount,
} from '../../lib/money'
import { dashboardKeys } from '../dashboard/queries'
import { quoteKeys } from '../quotes/queries'

const settingsKeys = {
  all: ['settings'] as const,
  impact: ['settings', 'impact'] as const,
}

/**
 * Configuración del sistema (RF-CFG-01 a 03), solo Administrador: datos de la empresa,
 * condiciones comerciales por defecto y parámetros comerciales. Antes de guardar los parámetros
 * se informa cuántos borradores se recalcularán; las cotizaciones emitidas no cambian (RN-26).
 */
export function SettingsPage() {
  const { data, error, isPending } = useQuery({
    queryKey: settingsKeys.all,
    queryFn: ({ signal }) => getSettings(signal),
  })

  return (
    <>
      <PageHeader
        title="Configuración"
        description={
          data ? `Última actualización: ${formatDateTime(data.updatedAt)}` : 'Datos del sistema'
        }
      />
      {isPending ? (
        <Spinner />
      ) : error ? (
        <Alert tone="danger">{getErrorMessage(error)}</Alert>
      ) : (
        <>
          <CompanySection settings={data} />
          <CommercialSection settings={data} />
        </>
      )}
    </>
  )
}

// ---------- Empresa y condiciones por defecto (RF-CFG-02) ----------

const companySchema = z.object({
  companyName: requiredText('El nombre de la empresa', COMPANY_NAME_MAX),
  companyLegalId: optionalText('La identificación', COMPANY_LEGAL_ID_MAX),
  companyEmail: optionalEmail,
  companyPhone: optionalPhone,
  companyAddress: optionalText('La dirección', COMPANY_ADDRESS_MAX),
  defaultCommercialTerms: optionalText(
    'Las condiciones comerciales por defecto',
    DEFAULT_TERMS_MAX,
  ),
})

type CompanyForm = z.infer<typeof companySchema>

const COMPANY_FIELDS = [
  'companyName',
  'companyLegalId',
  'companyEmail',
  'companyPhone',
  'companyAddress',
  'defaultCommercialTerms',
] as const

function companyDefaults(settings: SystemSettings): CompanyForm {
  return {
    companyName: settings.companyName,
    companyLegalId: settings.companyLegalId ?? '',
    companyEmail: settings.companyEmail ?? '',
    companyPhone: settings.companyPhone ? formatPhone(settings.companyPhone) : '',
    companyAddress: settings.companyAddress ?? '',
    defaultCommercialTerms: settings.defaultCommercialTerms ?? '',
  }
}

function CompanySection({ settings }: { settings: SystemSettings }) {
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<CompanyForm>({
    resolver: zodResolver(companySchema),
    mode: 'onChange',
    defaultValues: companyDefaults(settings),
  })
  const { errors, isDirty } = form.formState

  const save = useMutation({
    mutationFn: (values: CompanyForm) =>
      updateCompanySettings({
        companyName: values.companyName,
        companyLegalId: emptyToNull(values.companyLegalId),
        companyEmail: emptyToNull(values.companyEmail),
        companyPhone: emptyToNull(values.companyPhone),
        companyAddress: emptyToNull(values.companyAddress),
        defaultCommercialTerms: emptyToNull(values.defaultCommercialTerms),
      }),
    onSuccess: async (_, values) => {
      form.reset(values)
      await queryClient.invalidateQueries({ queryKey: settingsKeys.all })
      notify('Datos de la empresa actualizados.')
    },
    onError: (error) => setFormError(applyServerErrors(error, form.setError, COMPANY_FIELDS)),
  })

  return (
    <section className="card form-card settings-section" aria-labelledby="company-title">
      <h2 id="company-title" className="card__title">
        Empresa y condiciones por defecto
      </h2>
      <p className="card__text muted">
        Aparecen en el documento comercial. Las cotizaciones emitidas conservan los datos con los
        que se emitieron.
      </p>
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
            label="Nombre de la empresa"
            required
            maxLength={COMPANY_NAME_MAX}
            error={errors.companyName?.message}
            {...form.register('companyName')}
          />
          <TextField
            label="Identificación"
            maxLength={COMPANY_LEGAL_ID_MAX}
            error={errors.companyLegalId?.message}
            hint="Opcional. Cédula jurídica."
            {...form.register('companyLegalId')}
          />
          <TextField
            label="Correo electrónico"
            type="email"
            autoComplete="off"
            error={errors.companyEmail?.message}
            hint="Opcional."
            {...form.register('companyEmail')}
          />
          <TextField
            label="Teléfono"
            type="tel"
            autoComplete="off"
            error={errors.companyPhone?.message}
            hint="Opcional. 8 dígitos, con +506 opcional (por ejemplo, 2222-3333)."
            {...form.register('companyPhone')}
          />
          <div className="form-grid__wide">
            <TextField
              label="Dirección"
              maxLength={COMPANY_ADDRESS_MAX}
              error={errors.companyAddress?.message}
              hint="Opcional."
              {...form.register('companyAddress')}
            />
          </div>
          <div className="form-grid__wide">
            <TextAreaField
              label="Condiciones comerciales por defecto"
              maxLength={DEFAULT_TERMS_MAX}
              error={errors.defaultCommercialTerms?.message}
              hint="Opcional. Se copian a cada borrador nuevo, donde se pueden editar."
              {...form.register('defaultCommercialTerms')}
            />
          </div>
        </div>
        <div className="form-actions">
          <Button type="button" disabled={!isDirty || save.isPending} onClick={() => form.reset()}>
            Descartar cambios
          </Button>
          <Button type="submit" variant="primary" isLoading={save.isPending} disabled={!isDirty}>
            Guardar datos de la empresa
          </Button>
        </div>
      </form>
    </section>
  )
}

// ---------- Parámetros comerciales (RF-CFG-03) ----------

const percentageText = z
  .string()
  .refine((value) => parsePercentage(value) !== null, PERCENTAGE_FORMAT_MESSAGE)
const amountText = z.string().refine(isValidAmountText, AMOUNT_FORMAT_MESSAGE)

const commercialSchema = z
  .object({
    vatRate: percentageText,
    tier1Threshold: amountText,
    tier1DiscountRate: percentageText,
    tier2Threshold: amountText,
    tier2DiscountRate: percentageText,
    vipDiscountRate: percentageText,
    validityDays: z
      .string()
      .refine((value) => parseValidityDays(value) !== null, VALIDITY_DAYS_MESSAGE),
  })
  .superRefine((values, ctx) => {
    const parsed = toCommercialInput(values)
    if (!parsed) {
      return
    }
    for (const [field, message] of Object.entries(commercialPolicyErrors(parsed))) {
      ctx.addIssue({ code: 'custom', path: [field], message })
    }
  })

type CommercialForm = z.infer<typeof commercialSchema>

const COMMERCIAL_FIELDS = [
  'vatRate',
  'tier1Threshold',
  'tier1DiscountRate',
  'tier2Threshold',
  'tier2DiscountRate',
  'vipDiscountRate',
  'validityDays',
] as const

function toCommercialInput(values: CommercialForm): CommercialParametersInput | null {
  const parsed = {
    vatRate: parsePercentage(values.vatRate),
    tier1Threshold: parseAmount(values.tier1Threshold),
    tier1DiscountRate: parsePercentage(values.tier1DiscountRate),
    tier2Threshold: parseAmount(values.tier2Threshold),
    tier2DiscountRate: parsePercentage(values.tier2DiscountRate),
    vipDiscountRate: parsePercentage(values.vipDiscountRate),
    validityDays: parseValidityDays(values.validityDays),
  }
  return Object.values(parsed).every((value) => value !== null)
    ? (parsed as CommercialParametersInput)
    : null
}

function commercialDefaults(settings: SystemSettings): CommercialForm {
  return {
    vatRate: percentageToInput(settings.vatRate),
    tier1Threshold: amountToInput(settings.tier1Threshold),
    tier1DiscountRate: percentageToInput(settings.tier1DiscountRate),
    tier2Threshold: amountToInput(settings.tier2Threshold),
    tier2DiscountRate: percentageToInput(settings.tier2DiscountRate),
    vipDiscountRate: percentageToInput(settings.vipDiscountRate),
    validityDays: String(settings.validityDays),
  }
}

const days = (value: number) => `${value} ${value === 1 ? 'día' : 'días'}`

const PARAMETER_LABELS: Record<
  keyof CommercialParametersInput,
  { label: string; format: (value: number) => string }
> = {
  vatRate: { label: 'IVA', format: formatPercentage },
  tier1Threshold: { label: 'Umbral del tramo 1', format: formatCurrency },
  tier1DiscountRate: { label: 'Descuento del tramo 1', format: formatPercentage },
  tier2Threshold: { label: 'Umbral del tramo 2', format: formatCurrency },
  tier2DiscountRate: { label: 'Descuento del tramo 2', format: formatPercentage },
  vipDiscountRate: { label: 'Descuento VIP', format: formatPercentage },
  validityDays: { label: 'Vigencia', format: days },
}

function describeChanges(before: CommercialParametersInput, after: CommercialParametersInput) {
  return COMMERCIAL_FIELDS.filter((field) => before[field] !== after[field]).map((field) => {
    const { label, format } = PARAMETER_LABELS[field]
    return { field, label, before: format(before[field]), after: format(after[field]) }
  })
}

function draftsMessage(count: number): string {
  if (count === 0) {
    return 'No hay borradores: ninguna cotización se recalculará.'
  }
  return count === 1
    ? 'Se recalculará 1 borrador con los nuevos valores.'
    : `Se recalcularán ${count} borradores con los nuevos valores.`
}

function CommercialSection({ settings }: { settings: SystemSettings }) {
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const [formError, setFormError] = useState<string | null>(null)
  const [pending, setPending] = useState<CommercialParametersInput | null>(null)

  const form = useForm<CommercialForm>({
    resolver: zodResolver(commercialSchema),
    mode: 'onChange',
    defaultValues: commercialDefaults(settings),
  })
  const { errors, isDirty } = form.formState

  // Conteo de borradores afectados, consultado al abrir la confirmación (RF-CFG-03).
  const impact = useQuery({
    queryKey: settingsKeys.impact,
    queryFn: ({ signal }) => getCommercialParametersImpact(signal),
    enabled: pending !== null,
    staleTime: 0,
  })

  const save = useMutation({
    mutationFn: updateCommercialParameters,
    onSuccess: async (result) => {
      setPending(null)
      form.reset(form.getValues())
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: settingsKeys.all }),
        queryClient.invalidateQueries({ queryKey: quoteKeys.all }),
        queryClient.invalidateQueries({ queryKey: dashboardKeys.all }),
      ])
      notify(
        result.draftQuotesToRecalculate === 1
          ? 'Parámetros actualizados. 1 borrador usa ahora los nuevos valores.'
          : `Parámetros actualizados. ${result.draftQuotesToRecalculate} borradores usan ahora los nuevos valores.`,
      )
    },
    onError: (error) => {
      setPending(null)
      setFormError(applyServerErrors(error, form.setError, COMMERCIAL_FIELDS))
    },
  })

  const current: CommercialParametersInput = {
    vatRate: settings.vatRate,
    tier1Threshold: settings.tier1Threshold,
    tier1DiscountRate: settings.tier1DiscountRate,
    tier2Threshold: settings.tier2Threshold,
    tier2DiscountRate: settings.tier2DiscountRate,
    vipDiscountRate: settings.vipDiscountRate,
    validityDays: settings.validityDays,
  }
  const changes = pending ? describeChanges(current, pending) : []

  // Las reglas entre campos (RN-27) se revalidan al cambiar cualquiera de los campos relacionados.
  const register = (
    field: (typeof COMMERCIAL_FIELDS)[number],
    deps: (typeof COMMERCIAL_FIELDS)[number][] = [],
  ) => form.register(field, { deps })

  return (
    <section className="card form-card settings-section" aria-labelledby="commercial-title">
      <h2 id="commercial-title" className="card__title">
        Parámetros comerciales
      </h2>
      <p className="card__text muted">
        Los cambios se aplican a los borradores; las cotizaciones emitidas conservan los valores con
        los que se emitieron.
      </p>
      {formError && <Alert tone="danger">{formError}</Alert>}
      <form
        className="form"
        noValidate
        onSubmit={form.handleSubmit((values) => {
          setFormError(null)
          const input = toCommercialInput(values)
          if (input) {
            setPending(input)
          }
        })}
      >
        <div className="form-grid">
          <TextField
            label="IVA (%)"
            required
            inputMode="decimal"
            error={errors.vatRate?.message}
            hint="Se aplica sobre la base imponible."
            {...register('vatRate')}
          />
          <TextField
            label="Vigencia (días)"
            required
            inputMode="numeric"
            error={errors.validityDays?.message}
            hint="Entre 1 y 365 días desde la emisión."
            {...register('validityDays')}
          />
          <TextField
            label="Umbral del tramo 1 (₡)"
            required
            inputMode="decimal"
            error={errors.tier1Threshold?.message}
            hint="Subtotal a partir del cual se aplica el descuento del tramo 1."
            {...register('tier1Threshold', ['tier2Threshold'])}
          />
          <TextField
            label="Descuento del tramo 1 (%)"
            required
            inputMode="decimal"
            error={errors.tier1DiscountRate?.message}
            {...register('tier1DiscountRate', ['tier2DiscountRate'])}
          />
          <TextField
            label="Umbral del tramo 2 (₡)"
            required
            inputMode="decimal"
            error={errors.tier2Threshold?.message}
            hint="Debe ser mayor que el umbral del tramo 1."
            {...register('tier2Threshold', ['tier1Threshold'])}
          />
          <TextField
            label="Descuento del tramo 2 (%)"
            required
            inputMode="decimal"
            error={errors.tier2DiscountRate?.message}
            hint="Mayor o igual que el del tramo 1."
            {...register('tier2DiscountRate', ['tier1DiscountRate', 'vipDiscountRate'])}
          />
          <TextField
            label="Descuento VIP (%)"
            required
            inputMode="decimal"
            error={errors.vipDiscountRate?.message}
            hint="Se suma al del tramo; tramo 2 + VIP no puede superar 100 %."
            {...register('vipDiscountRate', ['tier2DiscountRate'])}
          />
        </div>
        <div className="form-actions">
          <Button type="button" disabled={!isDirty || save.isPending} onClick={() => form.reset()}>
            Descartar cambios
          </Button>
          <Button type="submit" variant="primary" disabled={!isDirty}>
            Guardar parámetros
          </Button>
        </div>
      </form>

      <ConfirmDialog
        open={pending !== null}
        title="¿Guardar los parámetros comerciales?"
        confirmLabel="Guardar parámetros"
        isLoading={save.isPending}
        onCancel={() => setPending(null)}
        onConfirm={() => {
          if (pending) {
            save.mutate(pending)
          }
        }}
      >
        {changes.length === 0 ? (
          <p>Los valores son iguales a los actuales.</p>
        ) : (
          <ul>
            {changes.map((change) => (
              <li key={change.field}>
                {change.label}: {change.before} → <strong>{change.after}</strong>
              </li>
            ))}
          </ul>
        )}
        {impact.isPending ? (
          <Spinner label="Consultando borradores afectados…" />
        ) : impact.error ? (
          <Alert tone="warning">
            No se pudo consultar cuántos borradores se recalcularán: {getErrorMessage(impact.error)}
          </Alert>
        ) : (
          <Alert tone="info" title={draftsMessage(impact.data.draftQuotesToRecalculate)}>
            Las cotizaciones emitidas, aceptadas, rechazadas, vencidas o canceladas no cambian.
          </Alert>
        )}
      </ConfirmDialog>
    </section>
  )
}
