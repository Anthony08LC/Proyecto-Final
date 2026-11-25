import { env } from '../config/env'
import { ApiError } from './apiError'

/**
 * Núcleo de acceso HTTP. Es el ÚNICO lugar del frontend que llama a `fetch`: los componentes usan
 * los módulos tipados de `src/api/*` a través de hooks de TanStack Query.
 */

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE'

type QueryValue = string | number | boolean | null | undefined

export interface RequestOptions {
  method?: HttpMethod
  /** Se envía como JSON. */
  body?: unknown
  /** Parámetros de la cadena de consulta; los vacíos (`undefined`, `null`, `''`) se omiten. */
  query?: Record<string, QueryValue>
  signal?: AbortSignal
  /** Códigos de error que no lanzan excepción (por ejemplo, 503 del health check). */
  acceptStatus?: readonly number[]
  /** Tipo de contenido esperado; por defecto JSON. */
  accept?: string
  /** `blob` para archivos (por ejemplo, el PDF); los errores siguen llegando como ProblemDetails. */
  responseType?: 'json' | 'blob'
}

export interface HttpHooks {
  /** Token de la sesión actual, si existe. */
  getToken: () => string | null
  /** Se invoca cuando una solicitud autenticada recibe 401 (sesión vencida o invalidada). */
  onUnauthorized: () => void
}

let hooks: HttpHooks = {
  getToken: () => null,
  onUnauthorized: () => {},
}

/** Conecta la capa HTTP con la sesión. La sesión lo configura al iniciar la aplicación. */
export function configureHttp(next: HttpHooks): void {
  hooks = next
}

/** Respuesta cruda, para los casos en que el estado importa (ver `acceptStatus`). */
export interface HttpResponse<T> {
  status: number
  data: T
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const response = await send<T>(path, options)
  return response.data
}

export async function send<T>(
  path: string,
  options: RequestOptions = {},
): Promise<HttpResponse<T>> {
  const {
    method = 'GET',
    body,
    query,
    signal,
    acceptStatus = [],
    accept = 'application/json',
    responseType = 'json',
  } = options
  const token = hooks.getToken()

  // Los errores llegan siempre como ProblemDetails (JSON), aunque se pida HTML o un PDF.
  const headers: Record<string, string> = {
    Accept: accept === 'application/json' ? accept : `${accept}, application/problem+json`,
  }
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  let response: Response
  try {
    response = await fetch(buildUrl(path, query), {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    })
  } catch (error) {
    // La cancelación (al desmontar un componente o repetir una consulta) no es un error de red.
    if (error instanceof DOMException && error.name === 'AbortError') {
      throw error
    }
    throw ApiError.network()
  }

  if (response.ok && responseType === 'blob') {
    return { status: response.status, data: (await response.blob()) as T }
  }

  const data = await readBody(response)

  if (!response.ok && !acceptStatus.includes(response.status)) {
    if (response.status === 401 && token) {
      hooks.onUnauthorized()
    }
    throw ApiError.fromResponse(response.status, data)
  }

  return { status: response.status, data: data as T }
}

function buildUrl(path: string, query: RequestOptions['query']): string {
  const url = new URL(`${env.apiUrl}${path}`)

  for (const [key, value] of Object.entries(query ?? {})) {
    if (value !== undefined && value !== null && value !== '') {
      url.searchParams.set(key, String(value))
    }
  }

  return url.toString()
}

async function readBody(response: Response): Promise<unknown> {
  if (response.status === 204) {
    return undefined
  }

  const text = await response.text()
  if (text === '') {
    return undefined
  }

  const contentType = response.headers.get('Content-Type') ?? ''
  if (contentType.includes('json')) {
    try {
      return JSON.parse(text)
    } catch {
      return undefined
    }
  }

  return text
}
