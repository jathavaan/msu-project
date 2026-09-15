import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BudgetCard } from './BudgetCard'
import type { Budget } from './types'

const budget: Budget = {
  id: 'b1',
  categoryId: 'c1',
  categoryName: 'Rent',
  monthlyLimit: 12000,
  usedThisMonth: 8000,
}

function renderCard(overrides: Partial<Budget> = {}) {
  const onEdit = vi.fn()
  const onDelete = vi.fn()
  render(<BudgetCard budget={{ ...budget, ...overrides }} onEdit={onEdit} onDelete={onDelete} />)
  return { onEdit, onDelete }
}

describe('BudgetCard', () => {
  it('shows the category name and how much of the limit is used', () => {
    renderCard()

    expect(screen.getByText('Rent')).toBeInTheDocument()
    expect(screen.getByText(/8 000,00/)).toBeInTheDocument()
    expect(screen.getByText(/12 000,00/)).toBeInTheDocument()
  })

  it('shows what is left when under budget', () => {
    renderCard()

    expect(screen.getByText(/left this month/)).toBeInTheDocument()
  })

  // Overspending flips the copy rather than showing a negative "left" amount, so the remainder is
  // displayed as its absolute value.
  it('shows the overspend when over budget', () => {
    renderCard({ usedThisMonth: 14000 })

    const remaining = screen.getByText(/over budget/)
    expect(remaining).toBeInTheDocument()
    expect(remaining).toHaveTextContent(/2 000,00/)
    expect(screen.queryByText(/left this month/)).not.toBeInTheDocument()
  })

  it('treats exactly hitting the limit as zero left, not over budget', () => {
    renderCard({ usedThisMonth: 12000 })

    expect(screen.getByText(/left this month/)).toBeInTheDocument()
  })

  it('hands the budget back to its edit and delete callbacks', async () => {
    const { onEdit, onDelete } = renderCard()

    const [edit, remove] = screen.getAllByRole('button')
    await userEvent.click(edit)
    await userEvent.click(remove)

    expect(onEdit).toHaveBeenCalledWith(budget)
    expect(onDelete).toHaveBeenCalledWith(budget)
  })
})
