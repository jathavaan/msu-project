import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Select } from './Select'

const OPTIONS = [
  { value: '0', label: 'Income' },
  { value: '1', label: 'Expense' },
]

describe('Select', () => {
  // The label is wired to the control by a generated id, which is what lets tests (and screen
  // readers) find the select by its visible label rather than by a test id.
  it('associates its label with the select', () => {
    render(<Select label="Category type" options={OPTIONS} onChange={vi.fn()} />)

    expect(screen.getByLabelText('Category type')).toBeInstanceOf(HTMLSelectElement)
  })

  it('renders every option', () => {
    render(<Select label="Type" options={OPTIONS} onChange={vi.fn()} />)

    expect(screen.getByRole('option', { name: 'Income' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'Expense' })).toBeInTheDocument()
  })

  // Read the value inside the handler: the select is controlled, so once React re-renders with the
  // unchanged `value` prop, the DOM node reverts and reading `target.value` afterwards lies.
  it('reports the selected value on change', async () => {
    const onChange = vi.fn()
    render(
      <Select label="Type" options={OPTIONS} value="1" onChange={(event) => onChange(event.target.value)} />,
    )

    await userEvent.selectOptions(screen.getByLabelText('Type'), '0')

    expect(onChange).toHaveBeenCalledWith('0')
  })

  it('renders a disabled placeholder when given one', () => {
    render(<Select label="Type" options={OPTIONS} placeholder="Pick one" value="" onChange={vi.fn()} />)

    expect(screen.getByRole('option', { name: 'Pick one' })).toBeDisabled()
  })

  it('shows a validation error under the field', () => {
    render(<Select label="Type" options={OPTIONS} error="Required" onChange={vi.fn()} />)

    expect(screen.getByText('Required')).toBeInTheDocument()
  })
})
