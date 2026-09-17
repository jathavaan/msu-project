import { describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen, waitFor } from '@testing-library/react'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { BalanceOverTimeChart } from './BalanceOverTimeChart'
import type { BalanceForecastPoint } from '../balance-forecast/types'

function stubForecast(points: BalanceForecastPoint[]) {
  server.use(
    http.get(`${API_URL}/balance-forecast`, () => HttpResponse.json({ result: points, errorCode: null, errorMessage: null })),
  )
}

describe('BalanceOverTimeChart', () => {
  it('renders a chart once the forecast loads, including a day with a saving', async () => {
    stubForecast([
      { day: 1, balance: 41_000, incomes: [], expenses: [], savings: [] },
      {
        day: 26,
        balance: 37_000,
        incomes: [],
        expenses: [],
        savings: [{ name: 'Buffer account', categoryName: 'Buffer', amount: 4_000 }],
      },
    ])
    const { container } = renderWithStore(<BalanceOverTimeChart />)

    await waitFor(() => expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument())
  })

  it('shows the empty state when nothing recurs', async () => {
    stubForecast([])
    renderWithStore(<BalanceOverTimeChart />)

    expect(await screen.findByText('No recurring income, expenses, or savings yet.')).toBeInTheDocument()
  })
})
