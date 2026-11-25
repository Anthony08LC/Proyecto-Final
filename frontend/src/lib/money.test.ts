import { describe, expect, it } from 'vitest'
import { amountToInput, isValidAmountText, parseAmount } from './money'

describe('parseAmount', () => {
  it.each([
    ['0', 0],
    ['1500000', 1500000],
    ['1500000,50', 1500000.5],
    ['1500000.5', 1500000.5],
    [' 99,99 ', 99.99],
    ['9999999999999,99', 9999999999999.99],
  ])('%s → %s', (text, expected) => {
    expect(parseAmount(text)).toBe(expected)
  })

  it.each(['', '-1', '1.500.000', '10,123', 'abc', '1e5', '12345678901234'])(
    'rechaza «%s»',
    (text) => {
      expect(parseAmount(text)).toBeNull()
      expect(isValidAmountText(text)).toBe(false)
    },
  )
})

describe('amountToInput', () => {
  it('usa coma decimal y 2 decimales', () => {
    expect(amountToInput(1500000.5)).toBe('1500000,50')
    expect(amountToInput(0)).toBe('0,00')
  })
})
