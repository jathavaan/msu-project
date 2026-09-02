/**
 * Deterministic id -> color mapping so a category reads the same everywhere (badges, table rows,
 * chart segments) without storing a color on the backend. Class names are literal strings (not
 * built from a template) so Tailwind's scanner picks them up.
 */
const PALETTE = [
  { bg: 'bg-cat-1', text: 'text-cat-1', dot: 'bg-cat-1' },
  { bg: 'bg-cat-2', text: 'text-cat-2', dot: 'bg-cat-2' },
  { bg: 'bg-cat-3', text: 'text-cat-3', dot: 'bg-cat-3' },
  { bg: 'bg-cat-4', text: 'text-cat-4', dot: 'bg-cat-4' },
  { bg: 'bg-cat-5', text: 'text-cat-5', dot: 'bg-cat-5' },
  { bg: 'bg-cat-6', text: 'text-cat-6', dot: 'bg-cat-6' },
  { bg: 'bg-cat-7', text: 'text-cat-7', dot: 'bg-cat-7' },
  { bg: 'bg-cat-8', text: 'text-cat-8', dot: 'bg-cat-8' },
] as const

const HEX_PALETTE = ['#16a34a', '#2563eb', '#d97706', '#db2777', '#7c3aed', '#0d9488', '#dc2626', '#4f46e5'] as const

function hashString(value: string): number {
  let hash = 0
  for (let i = 0; i < value.length; i++) {
    hash = (hash << 5) - hash + value.charCodeAt(i)
    hash |= 0
  }
  return Math.abs(hash)
}

export function categoryClasses(categoryId: string): (typeof PALETTE)[number] {
  return PALETTE[hashString(categoryId) % PALETTE.length]
}

/** Hex equivalent of the same palette, for Recharts fills (which can't consume Tailwind classes). */
export function categoryHex(categoryId: string): string {
  return HEX_PALETTE[hashString(categoryId) % HEX_PALETTE.length]
}
