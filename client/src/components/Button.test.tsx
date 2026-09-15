import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Button } from './Button'

describe('Button', () => {
  it('renders its children', () => {
    render(<Button>Add Budget</Button>)

    expect(screen.getByRole('button', { name: 'Add Budget' })).toBeInTheDocument()
  })

  it('calls onClick when pressed', async () => {
    const onClick = vi.fn()
    render(<Button onClick={onClick}>Save</Button>)

    await userEvent.click(screen.getByRole('button', { name: 'Save' }))

    expect(onClick).toHaveBeenCalledOnce()
  })

  it('does not call onClick while disabled', async () => {
    const onClick = vi.fn()
    render(
      <Button onClick={onClick} disabled>
        Save
      </Button>,
    )

    await userEvent.click(screen.getByRole('button', { name: 'Save' }))

    expect(onClick).not.toHaveBeenCalled()
  })

  // Forms rely on the implicit submit button, so the default type must stay "submit"-compatible:
  // Button sets no type itself, which means the browser default applies.
  it('passes arbitrary button attributes through', () => {
    render(<Button type="submit">Create category</Button>)

    expect(screen.getByRole('button', { name: 'Create category' })).toHaveAttribute('type', 'submit')
  })

  it('renders an icon alongside the label', () => {
    render(<Button icon={<span data-testid="icon" />}>Add</Button>)

    expect(screen.getByTestId('icon')).toBeInTheDocument()
    expect(screen.getByRole('button')).toHaveTextContent('Add')
  })
})
