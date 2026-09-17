import { describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { waitFor } from '@testing-library/react'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { SpendTrendChart } from './SpendTrendChart'
import type { CategorySpendTrend } from './types'

function stubTrend(trend: CategorySpendTrend) {
  server.use(
    http.get(`${API_URL}/spend-trends/c1`, () => HttpResponse.json({ result: trend, errorCode: null, errorMessage: null })),
  )
}

const months = [
  { year: 2026, month: 1, actual: 400 },
  { year: 2026, month: 2, actual: 350 },
]

describe('SpendTrendChart', () => {
  it('renders a chart once the trend loads', async () => {
    stubTrend({ categoryId: 'c1', categoryName: 'Food', monthlyLimit: null, months })
    const { container } = renderWithStore(<SpendTrendChart categoryId="c1" />)

    await waitFor(() => expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument())
  })

  it('renders a chart when the category has a budget too', async () => {
    stubTrend({ categoryId: 'c1', categoryName: 'Food', monthlyLimit: 500, months })
    const { container } = renderWithStore(<SpendTrendChart categoryId="c1" />)

    await waitFor(() => expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument())
  })

  // An empty months array shouldn't happen in practice (the handler always emits 6 entries), but
  // the component must not blow up recharts by handing it an empty dataset.
  it('renders nothing for an empty months array', async () => {
    stubTrend({ categoryId: 'c1', categoryName: 'Food', monthlyLimit: null, months: [] })
    const { container } = renderWithStore(<SpendTrendChart categoryId="c1" />)

    await waitFor(() => expect(container).toBeEmptyDOMElement())
  })
})
