import { describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen, waitFor } from '@testing-library/react'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { SavingGoalProjectionChart } from './SavingGoalProjectionChart'

function stubProjection(response: { points: { month: number; date: string; balance: number }[]; reachDate: string | null }) {
  server.use(
    http.get(`${API_URL}/saving-goals/g1/projection`, () =>
      HttpResponse.json({ result: response, errorCode: null, errorMessage: null }),
    ),
  )
}

describe('SavingGoalProjectionChart', () => {
  it('shows the reach date message once the projection loads', async () => {
    stubProjection({
      points: [
        { month: 0, date: '2026-06-15', balance: 80_000 },
        { month: 1, date: '2026-07-15', balance: 85_000 },
      ],
      reachDate: '2027-01-01',
    })
    renderWithStore(<SavingGoalProjectionChart savingGoalId="g1" targetAmount={250_000} />)

    expect(await screen.findByText('On track to reach this goal by Jan 1, 2027')).toBeInTheDocument()
  })

  it('shows the not-reachable message when reachDate is null', async () => {
    stubProjection({ points: [{ month: 0, date: '2026-06-15', balance: 0 }], reachDate: null })
    renderWithStore(<SavingGoalProjectionChart savingGoalId="g1" targetAmount={250_000} />)

    expect(await screen.findByText('Not reachable at the current rate')).toBeInTheDocument()
  })

  // An empty points array shouldn't happen in practice (the handler always emits at least month 0),
  // but the component must not blow up recharts by handing it an empty dataset.
  it('renders nothing for an empty points array', async () => {
    stubProjection({ points: [], reachDate: null })
    const { container } = renderWithStore(<SavingGoalProjectionChart savingGoalId="g1" targetAmount={250_000} />)

    await waitFor(() => expect(container).toBeEmptyDOMElement())
  })
})
