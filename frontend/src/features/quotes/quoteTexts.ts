/** Los cuatro textos comerciales de una cotización (RF-COT-05), en el orden del documento. */
export const QUOTE_TEXT_FIELDS = [
  { key: 'introduction', label: 'Introducción' },
  { key: 'executiveSummary', label: 'Resumen ejecutivo' },
  { key: 'notes', label: 'Observaciones' },
  { key: 'commercialTerms', label: 'Condiciones comerciales' },
] as const

export type QuoteTextKey = (typeof QUOTE_TEXT_FIELDS)[number]['key']
