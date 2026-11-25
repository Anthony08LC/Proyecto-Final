import { describe, expect, it } from 'vitest'
import {
  formatCurrency,
  formatDate,
  formatDateTime,
  formatPercentage,
  formatPhone,
  formatQuantity,
} from './format'

describe('formatCurrency', () => {
  it.each([
    [0, '₡0,00'],
    [0.01, '₡0,01'],
    [999.5, '₡999,50'],
    [1000, '₡1.000,00'],
    [500000.01, '₡500.000,01'],
    [1179720, '₡1.179.720,00'],
    [1044000, '₡1.044.000,00'],
    [123456789012.34, '₡123.456.789.012,34'],
  ])('%s → %s', (amount, expected) => {
    expect(formatCurrency(amount)).toBe(expected)
  })

  it('muestra el signo en montos negativos y no en un cero negativo', () => {
    expect(formatCurrency(-1500.5)).toBe('-₡1.500,50')
    expect(formatCurrency(-0)).toBe('₡0,00')
  })

  it('no rompe con valores no numéricos', () => {
    expect(formatCurrency(Number.NaN)).toBe('—')
  })
})

describe('formatPercentage', () => {
  it.each([
    [0, '0 %'],
    [13, '13 %'],
    [2.5, '2,5 %'],
    [10.25, '10,25 %'],
  ])('%s → %s', (value, expected) => {
    expect(formatPercentage(value)).toBe(expected)
  })
})

describe('formatQuantity', () => {
  it.each([
    [1, '1'],
    [1.5, '1,5'],
    [2.25, '2,25'],
    [1200, '1.200'],
  ])('%s → %s', (value, expected) => {
    expect(formatQuantity(value)).toBe(expected)
  })
})

describe('formatDate', () => {
  it('muestra una fecha de negocio sin convertir zona horaria', () => {
    expect(formatDate('2026-12-24')).toBe('24/12/2026')
  })

  it('convierte una fecha UTC a la fecha de Costa Rica (UTC−6) en el borde del día', () => {
    // 03:00 UTC del 25/11 es todavía el 24/11 a las 21:00 en Costa Rica.
    expect(formatDate('2026-11-25T03:00:00Z')).toBe('24/11/2026')
    expect(formatDate('2026-11-25T06:00:00Z')).toBe('25/11/2026')
  })

  it('cambia de año en Costa Rica, no en UTC', () => {
    expect(formatDate('2027-01-01T05:59:00+00:00')).toBe('31/12/2026')
  })

  it('devuelve un guion para valores vacíos o inválidos', () => {
    expect(formatDate(null)).toBe('—')
    expect(formatDate('no-es-fecha')).toBe('—')
  })
})

describe('formatDateTime', () => {
  it('usa la hora de Costa Rica en formato de 24 horas', () => {
    expect(formatDateTime('2026-11-25T03:05:00Z')).toBe('24/11/2026 21:05')
    expect(formatDateTime('2026-11-24T18:00:00.1234567+00:00')).toBe('24/11/2026 12:00')
  })
})

describe('formatPhone', () => {
  it('da formato al teléfono normalizado', () => {
    expect(formatPhone('+50688887777')).toBe('+506 8888-7777')
  })

  it('deja otros valores como están y usa un guion si no hay teléfono', () => {
    expect(formatPhone('2222-3333')).toBe('2222-3333')
    expect(formatPhone(null)).toBe('—')
  })
})
