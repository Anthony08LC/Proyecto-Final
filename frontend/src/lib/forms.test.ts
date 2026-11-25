import { describe, expect, it } from 'vitest'
import { isValidPhone, optionalEmail } from './forms'

describe('isValidPhone', () => {
  it.each(['88887777', '+50688887777', '8888-7777', '+506 8888 7777', '(+506) 8888-7777'])(
    'acepta %s',
    (value) => expect(isValidPhone(value)).toBe(true),
  )

  it.each(['8888777', '888877776', '+1 8888 7777', 'abcdefgh'])('rechaza %s', (value) =>
    expect(isValidPhone(value)).toBe(false),
  )
})

describe('optionalEmail', () => {
  it('acepta vacío y correos válidos', () => {
    expect(optionalEmail.safeParse('').success).toBe(true)
    expect(optionalEmail.safeParse(' Ana@Empresa.cr ').success).toBe(true)
  })

  it('rechaza correos mal formados', () => {
    expect(optionalEmail.safeParse('ana@empresa').success).toBe(false)
    expect(optionalEmail.safeParse('ana empresa@x.cr').success).toBe(false)
  })
})
