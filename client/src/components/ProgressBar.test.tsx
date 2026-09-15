import { describe, expect, it } from 'vitest'
import { render } from '@testing-library/react'
import { ProgressBar } from './ProgressBar'

/** The filled portion is the only child of the track, which is the rendered root. */
function fill(container: HTMLElement) {
  return container.firstElementChild!.firstElementChild as HTMLElement
}

describe('ProgressBar', () => {
  it('fills proportionally to value/max', () => {
    const { container } = render(<ProgressBar value={25} max={100} />)

    expect(fill(container)).toHaveStyle({ width: '25%' })
  })

  // A budget can be overspent, so value may exceed max. The bar clamps rather than overflowing
  // its track.
  it('clamps at 100% when over budget', () => {
    const { container } = render(<ProgressBar value={150} max={100} />)

    expect(fill(container)).toHaveStyle({ width: '100%' })
  })

  // max is a user-entered monthly limit, so guard against a divide-by-zero rendering NaN%.
  it('renders empty when max is zero', () => {
    const { container } = render(<ProgressBar value={10} max={0} />)

    expect(fill(container)).toHaveStyle({ width: '0%' })
  })

  it.each([
    [10, 'bg-positive'],
    [79, 'bg-positive'],
    [80, 'bg-warning'],
    [99, 'bg-warning'],
    [100, 'bg-negative'],
    [120, 'bg-negative'],
  ])('picks its tone from the ratio (%i%% -> %s)', (value, expectedClass) => {
    const { container } = render(<ProgressBar value={value} max={100} />)

    expect(fill(container)).toHaveClass(expectedClass)
  })

  it('lets an explicit tone override the ratio', () => {
    const { container } = render(<ProgressBar value={10} max={100} tone="negative" />)

    expect(fill(container)).toHaveClass('bg-negative')
  })
})
