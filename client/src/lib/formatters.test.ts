import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  categoryTypeLabel,
  daysUntil,
  formatCurrency,
  formatDate,
  formatMonthLabel,
  formatRecurrenceDay,
} from './formatters'
import { CategoryType } from './types'

describe('formatCurrency', () => {
  it('formats an amount as NOK', () => {
    const formatted = formatCurrency(1234.5)

    // Intl inserts locale-specific grouping and non-breaking spaces, so assert on the parts that
    // are actually part of the contract rather than on an exact string.
    expect(formatted).toContain('kr')
    expect(formatted).toContain('234,50')
  })

  // nb-NO renders a negative with U+2212 MINUS SIGN, not an ASCII hyphen.
  it('keeps the sign on a negative amount', () => {
    expect(formatCurrency(-100)).toMatch(/^[-−]/)
  })
})

describe('formatDate', () => {
  // The API sends DateOnly as YYYY-MM-DD. Parsing that with `new Date(isoDate)` alone would treat
  // it as UTC midnight and render the previous day west of Greenwich, which is why formatters.ts
  // appends T00:00:00 to force local time.
  it('formats an API date without shifting the day', () => {
    expect(formatDate('2026-06-15')).toBe('Jun 15, 2026')
  })

  it('formats the first day of the year', () => {
    expect(formatDate('2026-01-01')).toBe('Jan 1, 2026')
  })
})

describe('formatRecurrenceDay', () => {
  it.each([
    [1, '1st of the month'],
    [2, '2nd of the month'],
    [3, '3rd of the month'],
    [4, '4th of the month'],
    [21, '21st of the month'],
    [22, '22nd of the month'],
    [23, '23rd of the month'],
    [28, '28th of the month'],
  ])('renders %i with the right ordinal suffix', (day, expected) => {
    expect(formatRecurrenceDay(day)).toBe(expected)
  })

  // 11/12/13 are the classic ordinal trap: they end in 1/2/3 but take "th".
  it.each([
    [11, '11th of the month'],
    [12, '12th of the month'],
    [13, '13th of the month'],
  ])('renders the teens %i with "th"', (day, expected) => {
    expect(formatRecurrenceDay(day)).toBe(expected)
  })
})

describe('formatMonthLabel', () => {
  it('formats a 1-indexed month with its year', () => {
    expect(formatMonthLabel(2026, 6)).toBe('Jun 2026')
  })

  // month is 1-12 from the API, not JS Date's 0-11 — this pins the off-by-one conversion down at
  // both ends of the range.
  it('handles January and December without rolling into an adjacent year', () => {
    expect(formatMonthLabel(2026, 1)).toBe('Jan 2026')
    expect(formatMonthLabel(2026, 12)).toBe('Dec 2026')
  })
})

describe('categoryTypeLabel', () => {
  it('labels the numeric wire values', () => {
    expect(categoryTypeLabel(CategoryType.Income)).toBe('Income')
    expect(categoryTypeLabel(CategoryType.Expense)).toBe('Expense')
  })
})

describe('daysUntil', () => {
  afterEach(() => vi.useRealTimers())

  it.each([
    ['2026-06-20', 5],
    ['2026-06-15', 0],
    ['2026-06-10', -5],
  ])('counts whole days from today to %s', (date, expected) => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-15T13:45:00'))

    expect(daysUntil(date)).toBe(expected)
  })

  // The time of day is zeroed before comparing, so a target later today still counts as 0 days
  // rather than rounding to 1.
  it('ignores the time of day', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-15T23:59:00'))

    expect(daysUntil('2026-06-16')).toBe(1)
  })
})
