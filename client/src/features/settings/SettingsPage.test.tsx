import { describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { server } from '../../test/server'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { SettingsPage } from './SettingsPage'

function getSettings(periodStartDay: number) {
  return http.get(`${API_URL}/settings`, () =>
    HttpResponse.json({ result: { periodStartDay }, errorCode: null, errorMessage: null }),
  )
}

describe('SettingsPage', () => {
  it('prefills the period start day from the loaded settings', async () => {
    server.use(getSettings(25))
    renderWithStore(<SettingsPage />)

    expect(await screen.findByLabelText('Period start day')).toHaveValue('25')
  })

  it('shows the error banner when the request fails', async () => {
    server.use(
      http.get(`${API_URL}/settings`, () =>
        HttpResponse.json({ status: 500, title: 'An unexpected error occurred.' }, { status: 500 }),
      ),
    )
    renderWithStore(<SettingsPage />)

    expect(await screen.findByText('An unexpected error occurred.')).toBeInTheDocument()
  })

  it('sends the exact request body when saving a new period start day', async () => {
    let requestBody: unknown
    server.use(
      getSettings(1),
      http.put(`${API_URL}/settings`, async ({ request }) => {
        requestBody = await request.json()
        return new HttpResponse(null, { status: 204 })
      }),
    )
    renderWithStore(<SettingsPage />)

    const select = await screen.findByLabelText('Period start day')
    await userEvent.selectOptions(select, '25')
    await userEvent.click(screen.getByRole('button', { name: 'Save' }))

    await waitFor(() => expect(requestBody).toEqual({ periodStartDay: 25 }))
    expect(await screen.findByText('Saved.')).toBeInTheDocument()
  })

  it('shows the error banner and not the saved message when the save fails', async () => {
    server.use(
      getSettings(1),
      http.put(`${API_URL}/settings`, () =>
        HttpResponse.json({ status: 400, title: 'periodStartDay must be between 1 and 28.' }, { status: 400 }),
      ),
    )
    renderWithStore(<SettingsPage />)

    await screen.findByLabelText('Period start day')
    await userEvent.click(screen.getByRole('button', { name: 'Save' }))

    expect(await screen.findByText('periodStartDay must be between 1 and 28.')).toBeInTheDocument()
    expect(screen.queryByText('Saved.')).not.toBeInTheDocument()
  })
})
