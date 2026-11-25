import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import { z } from 'zod'
import { ApiError, getErrorMessage } from '../api/apiError'

/**
 * Validación de formularios por esquema (spec 3.1, DT-04). Las reglas replican las del backend
 * para dar retroalimentación inmediata; el backend valida siempre de nuevo (RNF-SEG-06).
 */

/** Texto obligatorio, sin espacios al inicio ni al final, con longitud máxima. */
export function requiredText(label: string, max: number) {
  return z
    .string()
    .trim()
    .min(1, `${label} es obligatorio.`)
    .max(max, `${label} no puede superar los ${max} caracteres.`)
}

/** Texto opcional con longitud máxima; vacío equivale a «sin valor». */
export function optionalText(label: string, max: number) {
  return z.string().trim().max(max, `${label} no puede superar los ${max} caracteres.`)
}

/** Igual que `Email` del dominio: formato básico, máximo 254 caracteres, sin distinguir mayúsculas. */
export const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[^@\s]+$/
export const EMAIL_MAX_LENGTH = 254

export const optionalEmail = z
  .string()
  .trim()
  .max(
    EMAIL_MAX_LENGTH,
    `El correo electrónico no puede superar los ${EMAIL_MAX_LENGTH} caracteres.`,
  )
  .refine((value) => value === '' || EMAIL_PATTERN.test(value), {
    message: 'El correo electrónico no tiene un formato válido.',
  })

/** Igual que `Phone` del dominio: 8 dígitos con +506 opcional; se ignoran espacios, guiones y paréntesis. */
const PHONE_SEPARATORS = /[\s\-().]/g
const PHONE_PATTERN = /^(?:\+506)?\d{8}$/

export function isValidPhone(value: string): boolean {
  return PHONE_PATTERN.test(value.replace(PHONE_SEPARATORS, ''))
}

export const optionalPhone = z
  .string()
  .trim()
  .refine((value) => value === '' || isValidPhone(value), {
    message: 'El teléfono debe tener 8 dígitos, con el prefijo +506 opcional.',
  })

/**
 * Igual que `PasswordPolicy` del backend (RNF-SEG-03): de 10 a 128 caracteres, con al menos una
 * letra y un número.
 */
export const PASSWORD_MIN_LENGTH = 10
export const PASSWORD_MAX_LENGTH = 128

export const passwordPolicy = z
  .string()
  .min(PASSWORD_MIN_LENGTH, `Debe tener al menos ${PASSWORD_MIN_LENGTH} caracteres.`)
  .max(PASSWORD_MAX_LENGTH, `No puede superar los ${PASSWORD_MAX_LENGTH} caracteres.`)
  .regex(/\p{L}/u, 'Debe incluir al menos una letra.')
  .regex(/\d/, 'Debe incluir al menos un número.')

/** Convierte el texto vacío en `null` para enviarlo a la API. */
export function emptyToNull(value: string): string | null {
  const trimmed = value.trim()
  return trimmed === '' ? null : trimmed
}

/**
 * Muestra los errores de la API en el formulario: los de validación (400) junto a su campo y el
 * resto como mensaje general. Devuelve el mensaje general, si corresponde.
 */
export function applyServerErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fields: readonly Path<T>[],
): string | null {
  if (!(error instanceof ApiError)) {
    return getErrorMessage(error)
  }

  let unmatched = false
  for (const [field, messages] of Object.entries(error.fieldErrors)) {
    const match = fields.find((name) => name.toLowerCase() === field.toLowerCase())
    if (match && messages[0]) {
      setError(match, { type: 'server', message: messages[0] })
    } else {
      unmatched = true
    }
  }

  return !error.hasFieldErrors || unmatched ? error.message : null
}
