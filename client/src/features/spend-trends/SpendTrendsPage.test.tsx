import { describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { SpendTrendsPage } from './SpendTrendsPage'
import type { Transaction } from '../transactions/types'

function stubTransactions(transactions: Transaction[]) {
  return http.get(`${API_URL}/transactions`, () =>
    HttpResponse.json({ result: transactions, errorCode: null, errorMessage: null }),
  )
}

function stubCategories() {
  return http.get(`${API_URL}/categories`, () =>
    HttpResponse.json({
      result: [{ id: 'c1', name: 'Food', type: 1 }],
      errorCode: null,
      errorMessage: null,
    }),
  )
}

const transaction: Transaction = {
  id: 't1',
  date: '2026-06-03',
  description: 'Grocery Store',
  amount: -450,
  categoryId: 'c1',
  categoryName: 'Food',
}

describe('SpendTrendsPage', () => {
  it('shows an import prompt when no transactions have ever been imported', async () => {
    server.use(stubTransactions([]))
    renderWithStore(<SpendTrendsPage />)

    expect(await screen.findByText('No imported transactions yet')).toBeInTheDocument()
  })

  it('shows the category picker once transactions exist', async () => {
    server.use(stubTransactions([transaction]), stubCategories())
    renderWithStore(<SpendTrendsPage />)

    expect(await screen.findByLabelText('Category')).toBeInTheDocument()
    expect(screen.getByText('Select a category to see its spend trend.')).toBeInTheDocument()
  })

  it('loads the trend chart once a category is selected', async () => {
    server.use(
      stubTransactions([transaction]),
      stubCategories(),
      http.get(`${API_URL}/spend-trends/c1`, () =>
        HttpResponse.json({
          result: { categoryId: 'c1', categoryName: 'Food', monthlyLimit: 500, months: [{ year: 2026, month: 6, actual: 450 }] },
          errorCode: null,
          errorMessage: null,
        }),
      ),
    )
    renderWithStore(<SpendTrendsPage />)

    await screen.findByLabelText('Category')
    await waitFor(() => expect(screen.getByRole('option', { name: 'Food' })).toBeInTheDocument())
    await userEvent.selectOptions(screen.getByLabelText('Category'), 'c1')

    await waitFor(() =>
      expect(document.querySelector('.recharts-responsive-container')).toBeInTheDocument(),
    )
  })

  it('shows the error banner when loading transactions fails', async () => {
    server.use(
      http.get(`${API_URL}/transactions`, () =>
        HttpResponse.json({ status: 500, title: 'An unexpected error occurred.' }, { status: 500 }),
      ),
    )
    renderWithStore(<SpendTrendsPage />)

    expect(await screen.findByText('An unexpected error occurred.')).toBeInTheDocument()
  })
})
