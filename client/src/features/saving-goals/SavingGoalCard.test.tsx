import { describe, expect, it, vi } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { SavingGoalCard } from './SavingGoalCard'
import type { SavingGoal } from './types'

const savingGoal: SavingGoal = {
  id: 'g1',
  name: 'New Car',
  targetAmount: 250_000,
  targetDate: '2027-06-15',
  amountSaved: 80_000,
  amountRemaining: 170_000,
  daysRemaining: 300,
  monthlyContribution: 5_000,
  interestRate: null,
  imageUrl: null,
}

function stubProjection(reachDate: string | null) {
  server.use(
    http.get(`${API_URL}/saving-goals/g1/projection`, () =>
      HttpResponse.json({
        result: { points: [{ month: 0, date: '2026-06-15', balance: 80_000 }], reachDate },
        errorCode: null,
        errorMessage: null,
      }),
    ),
  )
}

function renderCard(overrides: Partial<SavingGoal> = {}) {
  const onEdit = vi.fn()
  const onDelete = vi.fn()
  renderWithStore(<SavingGoalCard savingGoal={{ ...savingGoal, ...overrides }} onEdit={onEdit} onDelete={onDelete} />)
  return { onEdit, onDelete }
}

describe('SavingGoalCard', () => {
  it('shows the goal name and progress', () => {
    stubProjection('2027-01-01')
    renderCard()

    expect(screen.getByText('New Car')).toBeInTheDocument()
    expect(screen.getByText(/80 000,00/)).toBeInTheDocument()
    expect(screen.getByText(/250 000,00/)).toBeInTheDocument()
  })

  it('shows the projected reach date once loaded', async () => {
    stubProjection('2027-01-01')
    renderCard()

    expect(await screen.findByText('On track to reach this goal by Jan 1, 2027')).toBeInTheDocument()
  })

  it('shows the not-reachable message when the projection never gets there', async () => {
    stubProjection(null)
    renderCard()

    expect(await screen.findByText('Not reachable at the current rate')).toBeInTheDocument()
  })

  // A goal already at/over its target has nothing left to project — the card must not even
  // request the projection (MSW fails the test on an unstubbed request).
  it('skips the projection request once the goal is already met', () => {
    renderCard({ amountSaved: 250_000, amountRemaining: 0 })

    expect(screen.queryByText(/reach this goal/)).not.toBeInTheDocument()
  })

  it('hands the goal back to its edit and delete callbacks', async () => {
    stubProjection('2027-01-01')
    const { onEdit, onDelete } = renderCard()

    const [edit, remove] = screen.getAllByRole('button')
    await userEvent.click(edit)
    await userEvent.click(remove)

    expect(onEdit).toHaveBeenCalledWith(expect.objectContaining({ id: 'g1' }))
    expect(onDelete).toHaveBeenCalledWith(expect.objectContaining({ id: 'g1' }))
  })

  it('shows a placeholder icon when no image has been uploaded', () => {
    stubProjection('2027-01-01')
    renderCard()

    expect(screen.queryByAltText('')).not.toBeInTheDocument()
  })

  it('shows the uploaded image, resolved against the API base URL, when one is set', () => {
    stubProjection('2027-01-01')
    renderCard({ imageUrl: '/api/saving-goals/g1/image' })

    expect(screen.getByAltText('')).toHaveAttribute('src', `${API_URL}/saving-goals/g1/image`)
  })
})
