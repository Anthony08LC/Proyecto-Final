import { describe, expect, it } from 'vitest'
import { parseQuantity, quantityToInput } from './quantity'

describe('parseQuantity', () => {
  it.each([
    ['1', 1],
    ['2,25', 2.25],
    ['1.5', 1.5],
    ['0,01', 0.01],
    [' 10 ', 10],
  ])('%s → %s', (text, expected) => {
    expect(parseQuantity(text)).toBe(expected)
  })

  it.each(['0', '0,00', '-1', '1,234', '', 'dos', '12345678901'])('rechaza «%s»', (text) => {
    expect(parseQuantity(text)).toBeNull()
  })
})

describe('quantityToInput', () => {
  it('usa coma decimal', () => {
    expect(quantityToInput(2.5)).toBe('2,5')
    expect(quantityToInput(3)).toBe('3')
  })
})
