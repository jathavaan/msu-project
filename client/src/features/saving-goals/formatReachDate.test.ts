import { describe, expect, it } from 'vitest'
import { formatReachDateMessage } from './formatReachDate'

describe('formatReachDateMessage', () => {
  it('reports the formatted reach date when reachable', () => {
    expect(formatReachDateMessage('2027-03-15')).toBe('On track to reach this goal by Mar 15, 2027')
  })

  it('reports unreachable when there is no reach date', () => {
    expect(formatReachDateMessage(null)).toBe('Not reachable at the current rate')
  })
})
