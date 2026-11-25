import { request } from './http'
import type { QuoteTextKey } from '../features/quotes/quoteTexts'

/**
 * Asistente de redacción comercial con IA (sección 11). Sugiere o mejora los cuatro textos de un
 * borrador; la sugerencia NO se guarda: solo al aceptarla se guarda con `updateQuoteTexts`.
 */

export type AssistantTextType = 'Introduction' | 'ExecutiveSummary' | 'Notes' | 'CommercialTerms'
export type AssistantAction = 'Generate' | 'Improve'

/** Disponibilidad del asistente y cupo del usuario (`AssistantStatusDto`). */
export interface AssistantStatus {
  isAvailable: boolean
  /** Explicación cuando no está disponible. */
  message: string | null
  requestsPerHour: number
  remainingThisHour: number
}

export interface QuoteTextSuggestion {
  textType: AssistantTextType
  action: AssistantAction
  suggestion: string
  /** Texto original, solo en las mejoras. */
  original: string | null
  remainingThisHour: number
}

/** Campo del formulario → tipo de texto de la API. */
export const ASSISTANT_TEXT_TYPES: Record<QuoteTextKey, AssistantTextType> = {
  introduction: 'Introduction',
  executiveSummary: 'ExecutiveSummary',
  notes: 'Notes',
  commercialTerms: 'CommercialTerms',
}

export function getAssistantStatus(signal?: AbortSignal) {
  return request<AssistantStatus>('/api/assistant/status', { signal })
}

/** Pide una sugerencia; puede tardar hasta 20 s (límite del backend). */
export function suggestQuoteText(
  quoteId: string,
  textType: AssistantTextType,
  action: AssistantAction,
  currentText: string | null,
) {
  return request<QuoteTextSuggestion>(`/api/quotes/${quoteId}/assistant`, {
    method: 'POST',
    body: { textType, action, currentText },
  })
}
