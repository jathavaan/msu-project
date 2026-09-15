import { describe, expect, it } from 'vitest'
import { categoryClasses, categoryHex } from './categoryColor'

// Colours are derived from the category id rather than stored, so the only real guarantees are
// that the same id always maps to the same colour and that the result is always a valid palette
// entry. Asserting a specific colour for a specific id would just pin the hash function in place.
describe('categoryColor', () => {
  const id = '0f8d3a4e-1c22-4b7a-9a0e-2f6c1b8d5e11'

  it('maps an id to the same classes every time', () => {
    expect(categoryClasses(id)).toEqual(categoryClasses(id))
  })

  it('maps an id to the same hex every time', () => {
    expect(categoryHex(id)).toBe(categoryHex(id))
  })

  it('always returns a palette class triple', () => {
    const classes = categoryClasses(id)

    expect(classes.bg).toMatch(/^bg-cat-[1-8]$/)
    expect(classes.text).toMatch(/^text-cat-[1-8]$/)
    expect(classes.dot).toMatch(/^bg-cat-[1-8]$/)
  })

  it('always returns a hex colour', () => {
    expect(categoryHex(id)).toMatch(/^#[0-9a-f]{6}$/)
  })

  // hashString builds up a signed 32-bit value, so an id that overflows into a negative hash has
  // to be brought back into range before indexing — otherwise the lookup returns undefined.
  it('stays in range across many different ids', () => {
    for (let i = 0; i < 500; i++) {
      expect(categoryClasses(`category-${i}`)).toBeDefined()
      expect(categoryHex(`category-${i}`)).toMatch(/^#[0-9a-f]{6}$/)
    }
  })

  it('handles an empty id', () => {
    expect(categoryClasses('')).toBeDefined()
  })
})
