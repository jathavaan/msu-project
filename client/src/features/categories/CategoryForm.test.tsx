import { describe, expect, it, vi } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { CategoryForm } from './CategoryForm'
import { CategoryType } from '../../lib/types'

describe('CategoryForm', () => {
  it('defaults a new category to Expense', () => {
    renderWithStore(<CategoryForm onDone={vi.fn()} />)

    expect(screen.getByLabelText('Type')).toHaveValue(String(CategoryType.Expense))
    expect(screen.getByRole('button', { name: 'Create category' })).toBeInTheDocument()
  })

  it('prefills from the category being edited', () => {
    renderWithStore(
      <CategoryForm category={{ id: 'c1', name: 'Salary', type: CategoryType.Income }} onDone={vi.fn()} />,
    )

    expect(screen.getByLabelText('Name')).toHaveValue('Salary')
    expect(screen.getByLabelText('Type')).toHaveValue(String(CategoryType.Income))
    expect(screen.getByRole('button', { name: 'Save changes' })).toBeInTheDocument()
  })

  // The one wire-format gotcha in this codebase: CategoryType is a number on the wire, but a
  // <select> value is always a string on the DOM. The form has to convert back on submit, or the
  // API receives "1" and model binding rejects it.
  it('submits the type as a number, not the string from the select', async () => {
    let body: unknown
    server.use(
      http.post(`${API_URL}/categories`, async ({ request }) => {
        body = await request.json()
        return HttpResponse.json('new-id', { status: 201 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<CategoryForm onDone={onDone} />)

    await userEvent.type(screen.getByLabelText('Name'), 'Rent')
    await userEvent.selectOptions(screen.getByLabelText('Type'), String(CategoryType.Expense))
    await userEvent.click(screen.getByRole('button', { name: 'Create category' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(body).toEqual({ name: 'Rent', type: 1 })
  })

  it('PUTs to the category id when editing', async () => {
    let body: unknown
    server.use(
      http.put(`${API_URL}/categories/c1`, async ({ request }) => {
        body = await request.json()
        return new HttpResponse(null, { status: 204 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(
      <CategoryForm category={{ id: 'c1', name: 'Salary', type: CategoryType.Income }} onDone={onDone} />,
    )

    await userEvent.clear(screen.getByLabelText('Name'))
    await userEvent.type(screen.getByLabelText('Name'), 'Freelance')
    await userEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(body).toEqual({ id: 'c1', name: 'Freelance', type: 0 })
  })

  // A 409 comes back as ProblemDetails; apiBaseQuery turns that into an ApiError whose message is
  // the `detail` field. The form must surface it and stay open rather than calling onDone.
  it('shows the API error and stays open when the name is taken', async () => {
    server.use(
      http.post(`${API_URL}/categories`, () =>
        HttpResponse.json(
          { status: 409, title: 'Conflict', detail: 'A category with this name and type already exists.' },
          { status: 409 },
        ),
      ),
    )
    const onDone = vi.fn()
    renderWithStore(<CategoryForm onDone={onDone} />)

    await userEvent.type(screen.getByLabelText('Name'), 'Rent')
    await userEvent.click(screen.getByRole('button', { name: 'Create category' }))

    expect(
      await screen.findByText('A category with this name and type already exists.'),
    ).toBeInTheDocument()
    expect(onDone).not.toHaveBeenCalled()
  })

  // ValidationProblemDetails has an `errors` dictionary instead of `detail`; apiBaseQuery flattens
  // it into one message so the form does not need to know which shape came back.
  it('flattens a validation problem into a single message', async () => {
    server.use(
      http.post(`${API_URL}/categories`, () =>
        HttpResponse.json(
          { status: 400, errors: { Name: ['Name is required.'] } },
          { status: 400 },
        ),
      ),
    )
    renderWithStore(<CategoryForm onDone={vi.fn()} />)

    await userEvent.type(screen.getByLabelText('Name'), 'x')
    await userEvent.click(screen.getByRole('button', { name: 'Create category' }))

    expect(await screen.findByText('Name is required.')).toBeInTheDocument()
  })

  it('closes without calling the API when cancelled', async () => {
    const onDone = vi.fn()
    renderWithStore(<CategoryForm onDone={onDone} />)

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onDone).toHaveBeenCalledOnce()
  })
})
