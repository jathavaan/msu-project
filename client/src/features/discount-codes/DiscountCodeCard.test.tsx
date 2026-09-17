import { describe, expect, it, vi } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { API_URL, renderWithStore } from '../../test/renderWithStore'
import { DiscountCodeCard } from './DiscountCodeCard'
import type { DiscountCode } from './types'

const discountCode: DiscountCode = {
  id: 'd1',
  storeName: 'Digg Pizza',
  codeText: 'Digg25',
  codeImageUrl: null,
  expiryDate: '2027-06-15',
}

function renderCard(overrides: Partial<DiscountCode> = {}) {
  const onEdit = vi.fn()
  const onDelete = vi.fn()
  renderWithStore(<DiscountCodeCard discountCode={{ ...discountCode, ...overrides }} onEdit={onEdit} onDelete={onDelete} />)
  return { onEdit, onDelete }
}

describe('DiscountCodeCard', () => {
  it('shows the store name and code', () => {
    renderCard()

    expect(screen.getByText('Digg Pizza')).toBeInTheDocument()
    expect(screen.getByText('Digg25')).toBeInTheDocument()
  })

  it('shows a placeholder icon and no image when none has been uploaded', () => {
    renderCard()

    expect(screen.queryByAltText('Digg Pizza coupon')).not.toBeInTheDocument()
  })

  it('shows the uploaded image full-width, resolved against the API base URL, when one is set', () => {
    renderCard({ codeImageUrl: '/api/discount-codes/d1/image' })

    expect(screen.getByAltText('Digg Pizza coupon')).toHaveAttribute('src', `${API_URL}/discount-codes/d1/image`)
  })

  it('drops the header placeholder icon once an image is uploaded', () => {
    const { container } = renderWithStore(
      <DiscountCodeCard
        discountCode={{ ...discountCode, codeImageUrl: '/api/discount-codes/d1/image' }}
        onEdit={vi.fn()}
        onDelete={vi.fn()}
      />,
    )

    expect(container.querySelectorAll('svg').length).toBe(2) // pencil + trash only, no ticket icon
  })

  it('hands the code back to its edit and delete callbacks', async () => {
    const { onEdit, onDelete } = renderCard()

    const [edit, remove] = screen.getAllByRole('button')
    await userEvent.click(edit)
    await userEvent.click(remove)

    expect(onEdit).toHaveBeenCalledWith(expect.objectContaining({ id: 'd1' }))
    expect(onDelete).toHaveBeenCalledWith(expect.objectContaining({ id: 'd1' }))
  })
})
