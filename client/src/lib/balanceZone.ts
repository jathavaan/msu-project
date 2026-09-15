// Hex equivalents of --color-positive / --color-negative in index.css — Recharts fills can't
// consume CSS vars or Tailwind classes (same rationale as categoryColor.ts's HEX_PALETTE).
export const POSITIVE_BALANCE_COLOR = '#16a34a'
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
