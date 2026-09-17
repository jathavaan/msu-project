import { describe, expect, it, vi } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { DiscountCodeForm } from './DiscountCodeForm'
import type { DiscountCode } from './types'

const discountCode: DiscountCode = {
  id: 'd1',
  storeName: 'Digg Pizza',
  codeText: 'Digg25',
  codeImageUrl: null,
  expiryDate: '2027-06-15',
}

describe('DiscountCodeForm', () => {
  it('defaults a new code to empty fields', () => {
    renderWithStore(<DiscountCodeForm onDone={vi.fn()} />)

    expect(screen.getByLabelText('Store')).toHaveValue('')
    expect(screen.getByLabelText('Code')).toHaveValue('')
    expect(screen.getByRole('button', { name: 'Add code' })).toBeInTheDocument()
  })

  it('prefills every field from the code being edited', () => {
    renderWithStore(<DiscountCodeForm discountCode={discountCode} onDone={vi.fn()} />)

    expect(screen.getByLabelText('Store')).toHaveValue('Digg Pizza')
    expect(screen.getByLabelText('Code')).toHaveValue('Digg25')
    expect(screen.getByLabelText('Expiry date')).toHaveValue('2027-06-15')
    expect(screen.getByRole('button', { name: 'Save changes' })).toBeInTheDocument()
  })

  it('sends the request body with no image URL field when creating a code', async () => {
    let body: unknown
    server.use(
      http.post(`${API_URL}/discount-codes`, async ({ request }) => {
        body = await request.json()
        return HttpResponse.json('new-id', { status: 201 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<DiscountCodeForm onDone={onDone} />)

    await userEvent.type(screen.getByLabelText('Store'), 'Kiwi')
    await userEvent.type(screen.getByLabelText('Code'), 'KIWI10')
    await userEvent.type(screen.getByLabelText('Expiry date'), '2027-06-15')
    await userEvent.click(screen.getByRole('button', { name: 'Add code' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(body).toEqual({ storeName: 'Kiwi', codeText: 'KIWI10', expiryDate: '2027-06-15' })
  })

  it('PUTs every field when editing', async () => {
    let body: unknown
    server.use(
      http.put(`${API_URL}/discount-codes/d1`, async ({ request }) => {
        body = await request.json()
        return new HttpResponse(null, { status: 204 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<DiscountCodeForm discountCode={discountCode} onDone={onDone} />)

    await userEvent.clear(screen.getByLabelText('Code'))
    await userEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(body).toEqual({ id: 'd1', storeName: 'Digg Pizza', codeText: null, expiryDate: '2027-06-15' })
  })

  function stubCreateObjectUrl() {
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:preview')
  }

  it('uploads the selected image after creating a new code', async () => {
    stubCreateObjectUrl()
    server.use(http.post(`${API_URL}/discount-codes`, () => HttpResponse.json('new-id', { status: 201 })))
    let uploadedBody: string | null = null
    server.use(
      http.put(`${API_URL}/discount-codes/new-id/image`, async ({ request }) => {
        uploadedBody = await request.text()
        return new HttpResponse(null, { status: 204 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<DiscountCodeForm onDone={onDone} />)
    const file = new File(['code'], 'code.png', { type: 'image/png' })

    await userEvent.type(screen.getByLabelText('Store'), 'Kiwi')
    await userEvent.type(screen.getByLabelText('Expiry date'), '2027-06-15')
    await userEvent.upload(screen.getByLabelText('Code image (optional)'), file)
    await userEvent.click(screen.getByRole('button', { name: 'Add code' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(uploadedBody).toContain('filename="code.png"')
    expect(uploadedBody).toContain('code')
  })

  it('uploads the selected image after editing an existing code', async () => {
    stubCreateObjectUrl()
    server.use(http.put(`${API_URL}/discount-codes/d1`, () => new HttpResponse(null, { status: 204 })))
    let uploadedBody: string | null = null
    server.use(
      http.put(`${API_URL}/discount-codes/d1/image`, async ({ request }) => {
        uploadedBody = await request.text()
        return new HttpResponse(null, { status: 204 })
      }),
    )
    const onDone = vi.fn()
    renderWithStore(<DiscountCodeForm discountCode={discountCode} onDone={onDone} />)
    const file = new File(['code'], 'code.png', { type: 'image/png' })

    await userEvent.upload(screen.getByLabelText('Code image (optional)'), file)
    await userEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
    expect(uploadedBody).toContain('filename="code.png"')
  })

  // No file selected means no multipart request at all — MSW's onUnhandledRequest: 'error' fails
  // the test if the form tried to hit the image endpoint anyway.
  it('does not upload an image when none is selected', async () => {
    server.use(http.put(`${API_URL}/discount-codes/d1`, () => new HttpResponse(null, { status: 204 })))
    const onDone = vi.fn()
    renderWithStore(<DiscountCodeForm discountCode={discountCode} onDone={onDone} />)

    await userEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => expect(onDone).toHaveBeenCalledOnce())
  })

  it('shows the API error and stays open when validation fails', async () => {
    server.use(
      http.post(`${API_URL}/discount-codes`, () =>
        HttpResponse.json({ status: 400, errors: { ExpiryDate: ['Expiry date cannot be in the past.'] } }, { status: 400 }),
      ),
    )
    const onDone = vi.fn()
    renderWithStore(<DiscountCodeForm onDone={onDone} />)

    await userEvent.type(screen.getByLabelText('Store'), 'Kiwi')
    await userEvent.type(screen.getByLabelText('Expiry date'), '2020-01-01')
    await userEvent.click(screen.getByRole('button', { name: 'Add code' }))

    expect(await screen.findByText('Expiry date cannot be in the past.')).toBeInTheDocument()
    expect(onDone).not.toHaveBeenCalled()
  })

  it('closes without calling the API when cancelled', async () => {
    const onDone = vi.fn()
    renderWithStore(<DiscountCodeForm onDone={onDone} />)

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onDone).toHaveBeenCalledOnce()
  })
})
