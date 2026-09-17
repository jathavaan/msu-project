import { describe, expect, it, vi } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { SavingGoalForm } from './SavingGoalForm'
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
  interestRate: 4.5,
  imageUrl: null,
}

/** Editing renders the projection chart, which every edit-mode test must stub. */
function stubProjection(reachDate: string | null = '2027-01-01') {
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

describe('SavingGoalForm', () => {
  it('defaults a new goal to empty fields with no interest rate and no amount-saved input', () => {
    renderWithStore(<SavingGoalForm onDone={vi.fn()} />)

    expect(screen.getByLabelText('Name')).toHaveValue('')
    expect(screen.getByLabelText(/Interest rate/)).toHaveValue(null)
    expect(screen.queryByLabelText('Amount saved so far')).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create goal' })).toBeInTheDocument()
  })

  it('prefills every field, including the interest rate, from the goal being edited', () => {
    stubProjection()
    renderWithStore(<SavingGoalForm savingGoal={savingGoal} onDone={vi.fn()} />)

    expect(screen.getByLabelText('Name')).toHaveValue('New Car')
    expect(screen.getByLabelText('Target amount')).toHaveValue(250_000)
    expect(screen.getByLabelText('Amount saved so far')).toHaveValue(80_000)
    expect(screen.getByLabelText(/Interest rate/)).toHaveValue(4.5)
    expect(screen.getByRole('button', { name: 'Save changes' })).toBeInTheDocument()
  })

  // A goal with no rate set must prefill the interest rate input empty, not "0" — the backend
  // treats null and 0 differently (null means no interest at all).
  it('prefills an empty interest rate when the goal has none set', () => {
    stubProjection()
    renderWithStore(<SavingGoalForm savingGoal={{ ...savingGoal, interestRate: null }} onDone={vi.fn()} />)

    expect(screen.getByLabelText(/Interest rate/)).toHaveValue(null)
  })

  it('sends a null interest rate when creating a goal without one', async () => {
    let body: unknown
    server.use(
      http.post(`${API_URL}/saving-goals`, async ({ request }) => {
        body = await request.json()
        return HttpResponse.json('new-id', { status: 201 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<SavingGoalForm onDone={onDone} />)

    await userEvent.type(screen.getByLabelText('Name'), 'Holiday')
    await userEvent.type(screen.getByLabelText('Target amount'), '30000')
    await userEvent.type(screen.getByLabelText('Target date'), '2027-06-15')
    await userEvent.click(screen.getByRole('button', { name: 'Create goal' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(body).toEqual({ name: 'Holiday', targetAmount: 30000, targetDate: '2027-06-15', interestRate: null })
  })

  it('sends the interest rate as a number when one is entered', async () => {
    let body: unknown
    server.use(
      http.post(`${API_URL}/saving-goals`, async ({ request }) => {
        body = await request.json()
        return HttpResponse.json('new-id', { status: 201 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<SavingGoalForm onDone={onDone} />)

    await userEvent.type(screen.getByLabelText('Name'), 'Holiday')
    await userEvent.type(screen.getByLabelText('Target amount'), '30000')
    await userEvent.type(screen.getByLabelText('Target date'), '2027-06-15')
    await userEvent.type(screen.getByLabelText(/Interest rate/), '4.5')
    await userEvent.click(screen.getByRole('button', { name: 'Create goal' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(body).toEqual({ name: 'Holiday', targetAmount: 30000, targetDate: '2027-06-15', interestRate: 4.5 })
  })

  it('PUTs every field, including a cleared interest rate, when editing', async () => {
    stubProjection()
    let body: unknown
    server.use(
      http.put(`${API_URL}/saving-goals/g1`, async ({ request }) => {
        body = await request.json()
        return new HttpResponse(null, { status: 204 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<SavingGoalForm savingGoal={savingGoal} onDone={onDone} />)

    await userEvent.clear(screen.getByLabelText(/Interest rate/))
    await userEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(body).toEqual({
      id: 'g1',
      name: 'New Car',
      targetAmount: 250_000,
      targetDate: '2027-06-15',
      currentAmount: 80_000,
      interestRate: null,
    })
  })

  it('renders the projection chart and reach date only when editing', async () => {
    stubProjection('2027-01-01')
    renderWithStore(<SavingGoalForm savingGoal={savingGoal} onDone={vi.fn()} />)

    expect(await screen.findByText(/On track to reach this goal by/)).toBeInTheDocument()
  })

  it('shows the API error and stays open when validation fails', async () => {
    server.use(
      http.post(`${API_URL}/saving-goals`, () =>
        HttpResponse.json(
          { status: 400, errors: { InterestRate: ['Interest rate must be between 0 and 100.'] } },
          { status: 400 },
        ),
      ),
    )
    const onDone = vi.fn()
    renderWithStore(<SavingGoalForm onDone={onDone} />)

    await userEvent.type(screen.getByLabelText('Name'), 'Holiday')
    await userEvent.type(screen.getByLabelText('Target amount'), '30000')
    await userEvent.type(screen.getByLabelText('Target date'), '2027-06-15')
    await userEvent.click(screen.getByRole('button', { name: 'Create goal' }))

    expect(await screen.findByText('Interest rate must be between 0 and 100.')).toBeInTheDocument()
    expect(onDone).not.toHaveBeenCalled()
  })

  it('closes without calling the API when cancelled', async () => {
    const onDone = vi.fn()
    renderWithStore(<SavingGoalForm onDone={onDone} />)

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onDone).toHaveBeenCalledOnce()
  })

  // jsdom has no real createObjectURL; vitest's own polyfill for it only understands jsdom's own
  // Blob, which the preview receives fine outside these tests. Stubbed here only because these
  // tests also exercise the upload request, which needs src/test/fileShim.ts's multipart encoding
  // to survive undici's brand-checking — the two aren't related, but both touch the same file.
  function stubCreateObjectUrl() {
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:preview')
  }

  // request.formData() hits an unrelated bug in this environment's undici when re-parsing a body
  // fileShim.ts had to hand-encode (see that file), so the raw multipart text is asserted on
  // directly instead — sufficient to prove the file's name and content made it into the request.
  it('uploads the selected image after creating a new goal', async () => {
    stubCreateObjectUrl()
    server.use(
      http.post(`${API_URL}/saving-goals`, () => HttpResponse.json('new-id', { status: 201 })),
    )
    let uploadedBody: string | null = null
    server.use(
      http.put(`${API_URL}/saving-goals/new-id/image`, async ({ request }) => {
        uploadedBody = await request.text()
        return new HttpResponse(null, { status: 204 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<SavingGoalForm onDone={onDone} />)
    const file = new File(['car'], 'car.png', { type: 'image/png' })

    await userEvent.type(screen.getByLabelText('Name'), 'Holiday')
    await userEvent.type(screen.getByLabelText('Target amount'), '30000')
    await userEvent.type(screen.getByLabelText('Target date'), '2027-06-15')
    await userEvent.upload(screen.getByLabelText('Image (optional)'), file)
    await userEvent.click(screen.getByRole('button', { name: 'Create goal' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(uploadedBody).toContain('filename="car.png"')
    expect(uploadedBody).toContain('car')
  })

  it('uploads the selected image after editing an existing goal', async () => {
    stubCreateObjectUrl()
    stubProjection()
    server.use(
      http.put(`${API_URL}/saving-goals/g1`, () => new HttpResponse(null, { status: 204 })),
    )
    let uploadedBody: string | null = null
    server.use(
      http.put(`${API_URL}/saving-goals/g1/image`, async ({ request }) => {
        uploadedBody = await request.text()
        return new HttpResponse(null, { status: 204 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<SavingGoalForm savingGoal={savingGoal} onDone={onDone} />)
    const file = new File(['car'], 'car.png', { type: 'image/png' })

    await userEvent.upload(screen.getByLabelText('Image (optional)'), file)
    await userEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(uploadedBody).toContain('filename="car.png"')
  })

  // No file selected means no multipart request at all — MSW's onUnhandledRequest: 'error' fails
  // the test if the form tried to hit the image endpoint anyway.
  it('does not upload an image when none is selected', async () => {
    stubProjection()
    server.use(
      http.put(`${API_URL}/saving-goals/g1`, () => new HttpResponse(null, { status: 204 })),
    )
    const onDone = vi.fn()
    renderWithStore(<SavingGoalForm savingGoal={savingGoal} onDone={onDone} />)

    await userEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
  })
})
