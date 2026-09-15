import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Modal } from './Modal'

describe('Modal', () => {
  it('renders nothing when closed', () => {
    render(
      <Modal open={false} onClose={vi.fn()} title="Add Budget">
        <p>Form goes here</p>
      </Modal>,
    )

    expect(screen.queryByText('Add Budget')).not.toBeInTheDocument()
    expect(screen.queryByText('Form goes here')).not.toBeInTheDocument()
  })

  it('renders its title and children when open', () => {
    render(
      <Modal open onClose={vi.fn()} title="Add Budget">
        <p>Form goes here</p>
      </Modal>,
    )

    expect(screen.getByRole('heading', { name: 'Add Budget' })).toBeInTheDocument()
    expect(screen.getByText('Form goes here')).toBeInTheDocument()
  })

  // Both the backdrop and the X share the "Close" label, so dismissing works from either.
  it('closes from the backdrop and from the close button', async () => {
    const onClose = vi.fn()
    render(
      <Modal open onClose={onClose} title="Add Budget">
        <p>Form goes here</p>
      </Modal>,
    )

    const closers = screen.getAllByRole('button', { name: 'Close' })
    expect(closers).toHaveLength(2)

    await userEvent.click(closers[0])
    await userEvent.click(closers[1])

    expect(onClose).toHaveBeenCalledTimes(2)
  })
})
