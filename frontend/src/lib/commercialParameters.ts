import type { CommercialParametersInput } from '../api/settings'
import { MAX_VALIDITY_DAYS, MIN_VALIDITY_DAYS } from '../api/settings'

/**
 * Entrada y reglas de los parámetros comerciales (RF-CFG-03, RN-27). Replican las del backend
 * para validar mientras se escribe; el backend valida siempre de nuevo.
 */

/** Porcentaje de 0 a 100 con hasta 2 decimales, con coma o punto: `13`, `2,5`, `12.75`. */
const PERCENTAGE_PATTERN = /^\d{1,3}(?:[.,]\d{1,2})?$/

export const PERCENTAGE_FORMAT_MESSAGE =
  'Escriba un porcentaje entre 0 y 100 con 2 decimales como máximo (por ejemplo, 13 o 2,5).'

export function parsePercentage(text: string): number | null {
  const trimmed = text.trim()
  if (!PERCENTAGE_PATTERN.test(trimmed)) {
    return null
  }
  const value = Number(trimmed.replace(',', '.'))
  return value <= 100 ? value : null
}

/** Texto editable a partir de un porcentaje de la API: `2.5` → `2,5`, `13` → `13`. */
export function percentageToInput(value: number): string {
  return value
    .toFixed(2)
    .replace(/\.?0+$/, '')
    .replace('.', ',')
}

export const VALIDITY_DAYS_MESSAGE = `La vigencia debe estar entre ${MIN_VALIDITY_DAYS} y ${MAX_VALIDITY_DAYS} días.`

export function parseValidityDays(text: string): number | null {
  const trimmed = text.trim()
  if (!/^\d{1,3}$/.test(trimmed)) {
    return null
  }
  const value = Number(trimmed)
  return value >= MIN_VALIDITY_DAYS && value <= MAX_VALIDITY_DAYS ? value : null
}

export type CommercialParameterField = keyof CommercialParametersInput

/**
 * Reglas de RN-27 que relacionan varios campos, con el mismo mensaje y el mismo campo que el
 * backend: umbral 2 > umbral 1 > 0; % tramo 2 ≥ % tramo 1; % tramo 2 + % VIP ≤ 100.
 */
export function commercialPolicyErrors(
  values: CommercialParametersInput,
): Partial<Record<CommercialParameterField, string>> {
  const errors: Partial<Record<CommercialParameterField, string>> = {}

  if (values.tier1Threshold <= 0) {
    errors.tier1Threshold = 'El umbral del tramo 1 debe ser mayor que cero.'
  }
  if (values.tier2Threshold <= values.tier1Threshold) {
    errors.tier2Threshold = 'El umbral del tramo 2 debe ser mayor que el umbral del tramo 1.'
  }
  if (values.tier2DiscountRate < values.tier1DiscountRate) {
    errors.tier2DiscountRate =
      'El descuento del tramo 2 debe ser mayor o igual que el descuento del tramo 1.'
  }
  // Se compara en centésimas para evitar errores de coma flotante (por ejemplo, 90,1 + 9,9).
  if (
    Math.round(values.tier2DiscountRate * 100) + Math.round(values.vipDiscountRate * 100) >
    10_000
  ) {
    errors.vipDiscountRate =
      'La suma del descuento del tramo 2 y el descuento VIP no puede superar 100 %.'
  }

  return errors
}
