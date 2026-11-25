import { describe, expect, it } from 'vitest'
import { ApiError, getErrorMessage } from './apiError'

describe('ApiError.fromResponse', () => {
  it('usa el detalle del ProblemDetails y conserva los errores por campo', () => {
    const error = ApiError.fromResponse(400, {
      title: 'Datos no válidos',
      status: 400,
      detail: 'Uno o más datos no son válidos.',
      errors: { newPassword: ['Debe tener al menos 10 caracteres.'] },
      traceId: '00-abc',
    })

    expect(error.status).toBe(400)
    expect(error.message).toBe('Uno o más datos no son válidos.')
    expect(error.fieldErrors).toEqual({ newPassword: ['Debe tener al menos 10 caracteres.'] })
    expect(error.hasFieldErrors).toBe(true)
    expect(error.traceId).toBe('00-abc')
  })

  it('muestra la causa exacta de una regla de negocio (422)', () => {
    const error = ApiError.fromResponse(422, {
      detail: 'La cotización debe tener al menos una línea para emitirse.',
    })

    expect(error.message).toBe('La cotización debe tener al menos una línea para emitirse.')
  })

  it('reconoce el cambio obligatorio de contraseña', () => {
    const error = ApiError.fromResponse(403, {
      detail: 'Debe cambiar su contraseña temporal antes de continuar.',
      code: 'password_change_required',
    })

    expect(error.isPasswordChangeRequired).toBe(true)
  })

  it('nunca muestra detalles de un error 500', () => {
    const error = ApiError.fromResponse(500, { detail: 'NullReferenceException at ...' })

    expect(error.message).toBe('Ocurrió un error inesperado. Intente de nuevo más tarde.')
  })

  it('usa un mensaje por defecto cuando el cuerpo no es ProblemDetails', () => {
    expect(ApiError.fromResponse(404, 'Not Found').message).toBe('El recurso solicitado no existe.')
    expect(ApiError.fromResponse(409, undefined).message).toContain('Otro usuario modificó')
  })

  it('ignora un campo errors con formato inesperado', () => {
    const error = ApiError.fromResponse(400, { errors: { email: 'texto' } })

    expect(error.hasFieldErrors).toBe(false)
  })
})

describe('getErrorMessage', () => {
  it('explica la falta de conexión', () => {
    expect(getErrorMessage(ApiError.network())).toContain('No se pudo conectar con el servidor')
  })

  it('no expone errores que no vienen de la API', () => {
    expect(getErrorMessage(new TypeError('x is undefined'))).toBe(
      'Ocurrió un error inesperado. Intente de nuevo más tarde.',
    )
  })
})
