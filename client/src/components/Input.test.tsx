import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Input } from './Input'

describe('Input', () => {
  it('associates its label with the input', () => {
    render(<Input label="Monthly limit" onChange={vi.fn()} />)

    expect(screen.getByLabelText('Monthly limit')).toBeInstanceOf(HTMLInputElement)
  })

  // The generated id slugs the label, so two fields whose labels differ only by spacing would
  // collide — worth knowing the slug is what it is.
  it('derives its id from the label', () => {
    render(<Input label="Monthly limit" onChange={vi.fn()} />)

    expect(screen.getByLabelText('Monthly limit')).toHaveAttribute('id', 'monthly-limit')
  })

  it('prefers an explicit id over the derived one', () => {
    render(<Input label="Monthly limit" id="limit" onChange={vi.fn()} />)

    expect(screen.getByLabelText('Monthly limit')).toHaveAttribute('id', 'limit')
  })

  it('reports what the user typed', async () => {
    const onChange = vi.fn()
    render(<Input label="Name" value="" onChange={onChange} />)

    await userEvent.type(screen.getByLabelText('Name'), 'Rent')

    expect(onChange).toHaveBeenCalledTimes(4)
  })

  it('shows a validation error under the field', () => {
    render(<Input label="Name" error="Name is required" onChange={vi.fn()} />)

    expect(screen.getByText('Name is required')).toBeInTheDocument()
  })

  it('passes input attributes through', () => {
    render(<Input label="Monthly limit" type="number" min={1} onChange={vi.fn()} />)

    const input = screen.getByLabelText('Monthly limit')
    expect(input).toHaveAttribute('type', 'number')
    expect(input).toHaveAttribute('min', '1')
  })
})
