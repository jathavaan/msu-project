import { describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { waitFor } from '@testing-library/react'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { TotalSavingsProjectionChart } from './TotalSavingsProjectionChart'

function stubProjection(points: { month: number; date: string; totalBalance: number }[]) {
  server.use(
    http.get(`${API_URL}/saving-goals/projection`, () => HttpResponse.json({ result: points, errorCode: null, errorMessage: null })),
  )
}

describe('TotalSavingsProjectionChart', () => {
  it('renders a chart once the combined projection loads', async () => {
    stubProjection([
      { month: 0, date: '2026-06-15', totalBalance: 15_000 },
      { month: 1, date: '2026-07-15', totalBalance: 16_500 },
    ])
    const { container } = renderWithStore(<TotalSavingsProjectionChart years={5} />)

    await waitFor(() => expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument())
  })

  // An empty points array shouldn't happen in practice (the handler always emits at least month 0),
  // but the component must not blow up recharts by handing it an empty dataset.
  it('renders nothing for an empty points array', async () => {
    stubProjection([])
    const { container } = renderWithStore(<TotalSavingsProjectionChart years={5} />)

    await waitFor(() => expect(container).toBeEmptyDOMElement())
  })

  it('requests the projection for the given horizon', async () => {
    let requestedYears: string | null = null
    server.use(
      http.get(`${API_URL}/saving-goals/projection`, ({ request }) => {
        requestedYears = new URL(request.url).searchParams.get('years')
        return HttpResponse.json({
          result: [{ month: 0, date: '2026-06-15', totalBalance: 15_000 }],
          errorCode: null,
          errorMessage: null,
        })
      }),
    )
    const { container } = renderWithStore(<TotalSavingsProjectionChart years={20} />)

    await waitFor(() => expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument())
    expect(requestedYears).toBe('20')
  })
})
