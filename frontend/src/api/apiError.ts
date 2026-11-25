/**
 * Errores de la API. El backend responde todos los errores en formato ProblemDetails (RFC 9457,
 * RNF-MAN-05), con títulos y mensajes en español:
 *
 * | Estado | Origen en el backend                                   |
 * |--------|--------------------------------------------------------|
 * | 400    | Validación: `errors` agrupa los mensajes por campo     |
 * | 401    | Sin sesión, sesión vencida o credenciales incorrectas  |
 * | 403    | Sin permiso; `code: password_change_required` si debe cambiar la contraseña |
 * | 404    | Recurso inexistente                                    |
 * | 409    | Conflicto de concurrencia o de unicidad                |
 * | 422    | Regla de negocio incumplida (el `detail` explica la causa) |
 * | 429    | Demasiados intentos de inicio de sesión o límite del asistente de IA |
 * | 500    | Error inesperado, con mensaje genérico                 |
 * | 502-504 | Asistente de IA no configurado, con error o sin respuesta a tiempo (`detail` apto para mostrar) |
 */

/** Cuerpo ProblemDetails tal como lo envía el backend. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  code?: string
  /** Solo en errores de validación (400): mensajes por campo, con nombres en camelCase. */
  errors?: Record<string, string[]>
}

/** Código que el backend agrega al 403 cuando el usuario debe cambiar su contraseña (RF-AUT-07). */
export const PASSWORD_CHANGE_REQUIRED = 'password_change_required'

const NETWORK_ERROR_MESSAGE =
  'No se pudo conectar con el servidor. Verifique su conexión e intente de nuevo.'

const FALLBACK_MESSAGES: Record<number, string> = {
  400: 'Los datos enviados no son válidos.',
  401: 'Debe iniciar sesión o su sesión ya no es válida.',
  403: 'No tiene permiso para realizar esta acción.',
  404: 'El recurso solicitado no existe.',
  409: 'Otro usuario modificó estos datos. Vuelva a cargarlos e intente de nuevo.',
  422: 'La operación no cumple las reglas del negocio.',
  429: 'Se superó el límite de intentos. Espere un momento e intente de nuevo.',
}

const GENERIC_MESSAGE = 'Ocurrió un error inesperado. Intente de nuevo más tarde.'

/** Error de una llamada a la API, con un mensaje listo para mostrar al usuario. */
export class ApiError extends Error {
  /** Código HTTP; 0 si no hubo respuesta (sin conexión, CORS, servidor detenido). */
  readonly status: number
  readonly title: string | undefined
  /** Errores por campo (camelCase), para mostrarlos junto a cada campo del formulario. */
  readonly fieldErrors: Readonly<Record<string, string[]>>
  readonly code: string | undefined
  /** Identificador para correlacionar el error con los logs del servidor. */
  readonly traceId: string | undefined

  constructor(status: number, message: string, problem?: ProblemDetails) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.title = problem?.title
    this.fieldErrors = problem?.errors ?? {}
    this.code = problem?.code
    this.traceId = problem?.traceId
  }

  get isNetworkError(): boolean {
    return this.status === 0
  }

  get isUnauthorized(): boolean {
    return this.status === 401
  }

  get isPasswordChangeRequired(): boolean {
    return this.status === 403 && this.code === PASSWORD_CHANGE_REQUIRED
  }

  get hasFieldErrors(): boolean {
    return Object.keys(this.fieldErrors).length > 0
  }

  static network(): ApiError {
    return new ApiError(0, NETWORK_ERROR_MESSAGE)
  }

  /** Construye el error a partir del estado HTTP y el cuerpo recibido (que puede no ser JSON). */
  static fromResponse(status: number, body: unknown): ApiError {
    const problem = isProblemDetails(body) ? body : undefined
    return new ApiError(status, messageFor(status, problem), problem)
  }
}

/** Mensaje comprensible para cualquier error capturado (RNF-USA-04). */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message
  }

  return GENERIC_MESSAGE
}

function messageFor(status: number, problem: ProblemDetails | undefined): string {
  const detail = problem?.detail?.trim()

  // Los 500 traen un mensaje genérico del backend; en los demás el detalle explica la causa.
  // Los 502-504 del asistente de IA (sección 11.4) traen un detalle pensado para el usuario.
  if (status >= 500) {
    return status >= 502 && status <= 504 && detail ? detail : GENERIC_MESSAGE
  }

  if (detail) {
    return detail
  }

  return FALLBACK_MESSAGES[status] ?? problem?.title ?? GENERIC_MESSAGE
}

function isProblemDetails(value: unknown): value is ProblemDetails {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as Record<string, unknown>
  const optionalString = (key: string) =>
    candidate[key] === undefined || typeof candidate[key] === 'string'

  return (
    optionalString('title') &&
    optionalString('detail') &&
    optionalString('code') &&
    (candidate['errors'] === undefined || isFieldErrors(candidate['errors']))
  )
}

function isFieldErrors(value: unknown): value is Record<string, string[]> {
  return (
    typeof value === 'object' &&
    value !== null &&
    Object.values(value).every(
      (messages) => Array.isArray(messages) && messages.every((m) => typeof m === 'string'),
    )
  )
}
