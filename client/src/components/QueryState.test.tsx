import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { QueryState } from './QueryState'

// QueryState is the single place every feature page delegates loading/error/empty handling to, so
// the precedence between those states is what matters: loading wins over error, error over empty,
// and the children only render once none of them apply.
describe('QueryState', () => {
  it('renders children on the happy path', () => {
    render(
      <QueryState isLoading={false}>
        <p>Budgets</p>
      </QueryState>,
    )

    expect(screen.getByText('Budgets')).toBeInTheDocument()
  })

  it('renders a spinner while loading', () => {
    const { container } = render(
      <QueryState isLoading>
        <p>Budgets</p>
      </QueryState>,
    )

    expect(screen.queryByText('Budgets')).not.toBeInTheDocument()
    expect(container.querySelector('.animate-spin')).toBeInTheDocument()
  })

  it('renders the error message instead of the children', () => {
    render(
      <QueryState isLoading={false} error={{ status: 500, message: 'Database is down.' }}>
        <p>Budgets</p>
      </QueryState>,
    )

    expect(screen.getByText('Database is down.')).toBeInTheDocument()
    expect(screen.queryByText('Budgets')).not.toBeInTheDocument()
  })

  // A SerializedError (thrown outside the base query) has no `status`, so getErrorMessage has to
  // cope with both shapes and the fallback copy covers the rest.
  it('falls back to generic copy when the error carries no message', () => {
    render(
      <QueryState isLoading={false} error={{ name: 'TypeError' }}>
        <p>Budgets</p>
      </QueryState>,
    )

    expect(screen.getByText('Something went wrong. Please try again.')).toBeInTheDocument()
  })

  it('renders the empty state when there is nothing to show', () => {
    render(
      <QueryState isLoading={false} isEmpty empty={<p>No budgets yet</p>}>
        <p>Budgets</p>
      </QueryState>,
    )

    expect(screen.getByText('No budgets yet')).toBeInTheDocument()
    expect(screen.queryByText('Budgets')).not.toBeInTheDocument()
  })

  // isEmpty only takes effect when an `empty` node was supplied — otherwise the page would render
  // a blank area with no explanation.
  it('renders children when empty with no empty node to show', () => {
    render(
      <QueryState isLoading={false} isEmpty>
        <p>Budgets</p>
      </QueryState>,
    )

    expect(screen.getByText('Budgets')).toBeInTheDocument()
  })
})
