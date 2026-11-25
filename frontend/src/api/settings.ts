import { request } from './http'

/** Configuración del sistema (`SystemSettingsDto`, RF-CFG-01). Porcentajes de 0 a 100. */
export interface SystemSettings {
  companyName: string
  companyLegalId: string | null
  companyEmail: string | null
  companyPhone: string | null
  companyAddress: string | null
  defaultCommercialTerms: string | null
  vatRate: number
  tier1Threshold: number
  tier1DiscountRate: number
  tier2Threshold: number
  tier2DiscountRate: number
  vipDiscountRate: number
  validityDays: number
  updatedAt: string
}

export interface CompanySettingsInput {
  companyName: string
  companyLegalId: string | null
  companyEmail: string | null
  companyPhone: string | null
  companyAddress: string | null
  defaultCommercialTerms: string | null
}

export interface CommercialParametersInput {
  vatRate: number
  tier1Threshold: number
  tier1DiscountRate: number
  tier2Threshold: number
  tier2DiscountRate: number
  vipDiscountRate: number
  validityDays: number
}

/** Borradores que usarán los nuevos parámetros; las emitidas no cambian (RN-26). */
export interface CommercialParametersImpact {
  draftQuotesToRecalculate: number
}

/** Límites del dominio (`SystemSettings`). */
export const COMPANY_NAME_MAX = 150
export const COMPANY_LEGAL_ID_MAX = 20
export const COMPANY_ADDRESS_MAX = 300
export const DEFAULT_TERMS_MAX = 4000
export const MIN_VALIDITY_DAYS = 1
export const MAX_VALIDITY_DAYS = 365

export function getSettings(signal?: AbortSignal) {
  return request<SystemSettings>('/api/settings', { signal })
}

/** Datos de la empresa y condiciones por defecto (RF-CFG-02). */
export function updateCompanySettings(input: CompanySettingsInput) {
  return request<void>('/api/settings/company', { method: 'PUT', body: input })
}

/** Cuántos borradores se recalcularán antes de guardar los parámetros (RF-CFG-03). */
export function getCommercialParametersImpact(signal?: AbortSignal) {
  return request<CommercialParametersImpact>('/api/settings/commercial-parameters/impact', {
    signal,
  })
}

/** IVA, tramos, descuento VIP y vigencia (RF-CFG-03). Solo afecta a los borradores. */
export function updateCommercialParameters(input: CommercialParametersInput) {
  return request<CommercialParametersImpact>('/api/settings/commercial-parameters', {
    method: 'PUT',
    body: input,
  })
}
