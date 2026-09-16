import { describe, expect, it } from 'vitest'
import { zeroCrossingOffset, zeroFloorDomain } from './balanceZone'

describe('zeroCrossingOffset', () => {
  it('returns 0 when the whole range is at or below zero', () => {
    expect(zeroCrossingOffset(-100, -10)).toBe(0)
    expect(zeroCrossingOffset(-100, 0)).toBe(0)
  })

  it('returns 1 when the whole range is at or above zero', () => {
    expect(zeroCrossingOffset(10, 100)).toBe(1)
    expect(zeroCrossingOffset(0, 100)).toBe(1)
  })

  it('splits proportionally when the range crosses zero', () => {
    expect(zeroCrossingOffset(-100, 100)).toBe(0.5)
    expect(zeroCrossingOffset(-100, 300)).toBe(0.75)
    expect(zeroCrossingOffset(-300, 100)).toBe(0.25)
  })
})

describe('zeroFloorDomain', () => {
  it('floors an all-positive range at 0 instead of the data minimum', () => {
    expect(zeroFloorDomain([11_166, 41_166, 61_166])).toEqual([0, 61_166])
  })

  it('caps an all-negative range at 0 instead of the data maximum', () => {
    expect(zeroFloorDomain([-500, -100, -50])).toEqual([-500, 0])
  })

  it('leaves a range that already crosses zero untouched', () => {
    expect(zeroFloorDomain([-9_417, 5_000, 40_583])).toEqual([-9_417, 40_583])
  })

  it('widens to a 1-wide domain when every value is exactly 0', () => {
    expect(zeroFloorDomain([0, 0, 0])).toEqual([0, 1])
  })
})
