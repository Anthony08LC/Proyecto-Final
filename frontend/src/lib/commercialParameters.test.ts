import { describe, expect, it } from 'vitest'
import type { CommercialParametersInput } from '../api/settings'
import {
  commercialPolicyErrors,
  parsePercentage,
  parseValidityDays,
  percentageToInput,
} from './commercialParameters'

const DEFAULTS: CommercialParametersInput = {
  vatRate: 13,
  tier1Threshold: 500_000,
  tier1DiscountRate: 5,
  tier2Threshold: 1_000_000,
  tier2DiscountRate: 10,
  vipDiscountRate: 3,
  validityDays: 30,
}

describe('parsePercentage', () => {
  it.each([
    ['0', 0],
    ['13', 13],
    ['2,5', 2.5],
    ['12.75', 12.75],
    ['100', 100],
  ])('%s → %s', (text, expected) => {
    expect(parsePercentage(text)).toBe(expected)
  })

  it.each(['', '-1', '100,01', '101', '2,555', 'abc'])('rechaza «%s»', (text) => {
    expect(parsePercentage(text)).toBeNull()
  })

  it('da formato editable', () => {
    expect(percentageToInput(13)).toBe('13')
    expect(percentageToInput(2.5)).toBe('2,5')
  })
})

describe('parseValidityDays', () => {
  it.each([
    ['1', 1],
    ['30', 30],
    ['365', 365],
  ])('%s → %s', (text, expected) => {
    expect(parseValidityDays(text)).toBe(expected)
  })

  it.each(['0', '366', '1,5', '', '-3'])('rechaza «%s»', (text) => {
    expect(parseValidityDays(text)).toBeNull()
  })
})

describe('commercialPolicyErrors (RN-27)', () => {
  it('acepta la configuración inicial', () => {
    expect(commercialPolicyErrors(DEFAULTS)).toEqual({})
  })

  it('umbral 1 mayor que cero', () => {
    expect(commercialPolicyErrors({ ...DEFAULTS, tier1Threshold: 0 })).toHaveProperty(
      'tier1Threshold',
    )
  })

  it('umbral 2 mayor que umbral 1', () => {
    expect(commercialPolicyErrors({ ...DEFAULTS, tier2Threshold: 500_000 })).toEqual({
      tier2Threshold: 'El umbral del tramo 2 debe ser mayor que el umbral del tramo 1.',
    })
  })

  it('% tramo 2 ≥ % tramo 1 (igual se acepta)', () => {
    expect(commercialPolicyErrors({ ...DEFAULTS, tier2DiscountRate: 5 })).toEqual({})
    expect(commercialPolicyErrors({ ...DEFAULTS, tier2DiscountRate: 4.99 })).toHaveProperty(
      'tier2DiscountRate',
    )
  })

  it('% tramo 2 + % VIP ≤ 100', () => {
    expect(
      commercialPolicyErrors({ ...DEFAULTS, tier2DiscountRate: 90.1, vipDiscountRate: 9.9 }),
    ).toEqual({})
    expect(
      commercialPolicyErrors({ ...DEFAULTS, tier2DiscountRate: 90.1, vipDiscountRate: 10 }),
    ).toHaveProperty('vipDiscountRate')
  })
})
