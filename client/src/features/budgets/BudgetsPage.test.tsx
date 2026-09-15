import { describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { BudgetsPage } from './BudgetsPage'
import type { Budget } from './types'

const budgets: Budget[] = [
  { id: 'b1', categoryId: 'c1', categoryName: 'Rent', monthlyLimit: 12000, usedThisMonth: 12000 },
  { id: 'b2', categoryId: 'c2', categoryName: 'Food & Drinks', monthlyLimit: 5000, usedThisMonth: 1250 },
]

/** GET /budgets wrapped in the backend's Response<T> envelope, which apiBaseQuery unwraps. */
function getBudgets(result: Budget[]) {
  return http.get(`${API_URL}/budgets`, () =>
    HttpResponse.json({ result, errorCode: null, errorMessage: null }),
  )
}

describe('BudgetsPage', () => {
  // This is the envelope contract in miniature: the API returns { result, errorCode, errorMessage }
  // but the page only ever sees the unwrapped array.
  it('renders a card per budget from the envelope', async () => {
    server.use(getBudgets(budgets))
    renderWithStore(<BudgetsPage />)

    expect(await screen.findByText('Rent')).toBeInTheDocument()
    expect(screen.getByText('Food & Drinks')).toBeInTheDocument()
  })

  it('shows the empty state when there are no budgets', async () => {
    server.use(getBudgets([]))
    renderWithStore(<BudgetsPage />)

    expect(await screen.findByText('No budgets yet')).toBeInTheDocument()
  })

  // An errorCode inside an otherwise-200 envelope is still a failure — apiBaseQuery turns it into
  // an ApiError, so the page must show the banner rather than rendering an empty grid.
  it('shows the error banner when the envelope carries an error code', async () => {
    server.use(
      http.get(`${API_URL}/budgets`, () =>
        HttpResponse.json({ result: null, errorCode: 500, errorMessage: 'Database is down.' }),
      ),
    )
    renderWithStore(<BudgetsPage />)

    expect(await screen.findByText('Database is down.')).toBeInTheDocument()
  })

  it('shows the error banner when the request itself fails', async () => {
    server.use(
      http.get(`${API_URL}/budgets`, () =>
        HttpResponse.json({ status: 500, title: 'An unexpected error occurred.' }, { status: 500 }),
      ),
    )
    renderWithStore(<BudgetsPage />)

    expect(await screen.findByText('An unexpected error occurred.')).toBeInTheDocument()
  })

  it('opens the create modal from the header button', async () => {
    server.use(getBudgets(budgets), http.get(`${API_URL}/categories`, () =>
      HttpResponse.json({ result: [], errorCode: null, errorMessage: null }),
    ))
    renderWithStore(<BudgetsPage />)

    await userEvent.click(await screen.findByRole('button', { name: /Add Budget/ }))

    expect(await screen.findByRole('heading', { name: 'Add Budget' })).toBeInTheDocument()
  })

  // Deleting is behind a confirm dialog, and cancelling it must not fire the DELETE. The MSW
  // server is configured to error on unhandled requests, so an accidental call fails the test.
  it('does not delete when the confirmation is cancelled', async () => {
    server.use(getBudgets(budgets))
    renderWithStore(<BudgetsPage />)

    await screen.findByText('Rent')
    const cards = screen.getAllByRole('button')
    // Each card renders an edit then a delete button after the header's "Add Budget".
    await userEvent.click(cards[2])

    expect(await screen.findByText(/Delete the budget for "Rent"\?/)).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    await waitFor(() =>
      expect(screen.queryByText(/Delete the budget for "Rent"\?/)).not.toBeInTheDocument(),
    )
  })

  it('sends the DELETE when the confirmation is accepted', async () => {
    let deletedId: string | undefined
    server.use(
      getBudgets(budgets),
      http.delete(`${API_URL}/budgets/:id`, ({ params }) => {
        deletedId = params.id as string
        return new HttpResponse(null, { status: 204 })
      }),
    )
    renderWithStore(<BudgetsPage />)

    await screen.findByText('Rent')
    await userEvent.click(screen.getAllByRole('button')[2])
    await userEvent.click(await screen.findByRole('button', { name: 'Delete' }))

    await waitFor(() => expect(deletedId).toBe('b1'))
  })
})
