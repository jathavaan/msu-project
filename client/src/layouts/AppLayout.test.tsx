import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { AppLayout } from './AppLayout'

function renderAppLayout() {
  return render(
    <MemoryRouter initialEntries={['/']}>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<div>Page Content</div>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  )
}

describe('AppLayout', () => {
  it('renders the routed page content', () => {
    renderAppLayout()

    expect(screen.getByText('Page Content')).toBeInTheDocument()
  })

  it('sidebar starts closed and has no overlay', () => {
    renderAppLayout()

    expect(screen.getByRole('link', { name: /dashboard/i }).closest('aside')).toHaveClass('-translate-x-full')
    expect(screen.queryByRole('button', { name: 'Close menu overlay' })).not.toBeInTheDocument()
  })

  it('opens the sidebar and shows an overlay when the menu button is clicked', async () => {
    renderAppLayout()

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }))

    expect(screen.getByRole('link', { name: /dashboard/i }).closest('aside')).toHaveClass('translate-x-0')
    expect(screen.getByRole('button', { name: 'Close menu overlay' })).toBeInTheDocument()
  })

  it('closes the sidebar when the overlay is clicked', async () => {
    renderAppLayout()

    await userEvent.click(screen.getByRole('button', { name: 'Open menu' }))
    await userEvent.click(screen.getByRole('button', { name: 'Close menu overlay' }))

    expect(screen.getByRole('link', { name: /dashboard/i }).closest('aside')).toHaveClass('-translate-x-full')
    expect(screen.queryByRole('button', { name: 'Close menu overlay' })).not.toBeInTheDocument()
  })
})
