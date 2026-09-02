import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../components/Input'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { useCreateDiscountCodeMutation, useUpdateDiscountCodeMutation } from './api'
import type { DiscountCode } from './types'

interface DiscountCodeFormProps {
  /** Omit to add a new discount code; pass an existing one to edit it. */
  discountCode?: DiscountCode
  onDone: () => void
}

export function DiscountCodeForm({ discountCode, onDone }: DiscountCodeFormProps) {
  const [storeName, setStoreName] = useState(discountCode?.storeName ?? '')
  const [codeText, setCodeText] = useState(discountCode?.codeText ?? '')
  const [codeImageUrl, setCodeImageUrl] = useState(discountCode?.codeImageUrl ?? '')
  const [expiryDate, setExpiryDate] = useState(discountCode?.expiryDate ?? '')

  const [createDiscountCode, { isLoading: isCreating, error: createError }] = useCreateDiscountCodeMutation()
  const [updateDiscountCode, { isLoading: isUpdating, error: updateError }] = useUpdateDiscountCodeMutation()

  const isSaving = isCreating || isUpdating
  const errorMessage = getErrorMessage(createError ?? updateError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const body = {
      storeName,
      codeText: codeText || null,
      codeImageUrl: codeImageUrl || null,
      expiryDate,
    }
    try {
      if (discountCode) {
        await updateDiscountCode({ id: discountCode.id, ...body }).unwrap()
      } else {
        await createDiscountCode(body).unwrap()
      }
      onDone()
    } catch {
      // Surfaced via the mutation's `error` state below.
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <Input label="Store" value={storeName} onChange={(event) => setStoreName(event.target.value)} required autoFocus />
      <Input label="Code" value={codeText} onChange={(event) => setCodeText(event.target.value)} placeholder="e.g. SAVE20" />
      <Input
        label="Code image URL"
        value={codeImageUrl}
        onChange={(event) => setCodeImageUrl(event.target.value)}
        placeholder="Optional link to a photo of the code"
      />
      <Input
        label="Expiry date"
        type="date"
        value={expiryDate}
        onChange={(event) => setExpiryDate(event.target.value)}
        required
      />
      {errorMessage && <ErrorBanner message={errorMessage} />}
      <div className="flex justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onDone}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSaving}>
          {discountCode ? 'Save changes' : 'Add code'}
        </Button>
      </div>
    </form>
  )
}
