// Hex equivalents of --color-cat-2 (blue) / --color-negative in index.css — Recharts fills can't
// consume CSS vars or Tailwind classes (same rationale as categoryColor.ts's HEX_PALETTE). Blue
// rather than the --color-positive green: it's the balance chart's original color and reads
// better for a running balance than green, which this app otherwise reserves for income/positive
// deltas specifically (see the tooltip's Income/Expenses labels).
export const POSITIVE_BALANCE_COLOR = '#2563eb'
export const NEGATIVE_BALANCE_COLOR = '#dc2626'

/**
 * Fraction (0-1) down a top-to-bottom gradient spanning [max, min] where the value crosses zero.
 * Used to split a chart's stroke/fill into a positive color above zero and a negative color below
 * at the exact point the value does, rather than an arbitrary fixed offset.
 */
export function zeroCrossingOffset(min: number, max: number): number {
  if (max <= 0) {
    return 0
  }
  if (min >= 0) {
    return 1
  }
  return max / (max - min)
}

/**
 * Y-axis domain that always includes 0 — as the floor once every value is positive, or extending
 * below it on a period where a value actually dips negative — rather than being pinned to the
 * data's own min/max, which would make the axis floor look like an arbitrary baseline instead of
 * an actual value of zero. Falls back to a 1-wide domain around 0 when every value is exactly 0,
 * since a chart can't render a zero-height domain.
 */
export function zeroFloorDomain(values: number[]): [min: number, max: number] {
  const min = Math.min(0, ...values)
  const max = Math.max(0, ...values)
  return min === max ? [min, max + 1] : [min, max]
}
