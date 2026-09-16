import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { Sidebar } from './Sidebar'

function renderSidebar(open: boolean, onClose = vi.fn()) {
  return render(
    <MemoryRouter>
      <Sidebar open={open} onClose={onClose} />
    </MemoryRouter>,
  )
}

describe('Sidebar', () => {
  it('renders all nav items', () => {
    renderSidebar(false)

    expect(screen.getByRole('link', { name: /dashboard/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /income/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /categories/i })).toBeInTheDocument()
  })

  it('is translated off-screen when closed and on-screen when open', () => {
    const { rerender } = renderSidebar(false)
    expect(screen.getByRole('link', { name: /dashboard/i }).closest('aside')).toHaveClass('-translate-x-full')

    rerender(
      <MemoryRouter>
        <Sidebar open onClose={vi.fn()} />
      </MemoryRouter>,
    )
    expect(screen.getByRole('link', { name: /dashboard/i }).closest('aside')).toHaveClass('translate-x-0')
  })

  it('calls onClose when the close button is clicked', async () => {
    const onClose = vi.fn()
    renderSidebar(true, onClose)

    await userEvent.click(screen.getByRole('button', { name: 'Close menu' }))

    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('calls onClose when a nav link is clicked', async () => {
    const onClose = vi.fn()
    renderSidebar(true, onClose)

    await userEvent.click(screen.getByRole('link', { name: /income/i }))

    expect(onClose).toHaveBeenCalledTimes(1)
  })
})
