import { useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'
import { Ticket } from 'lucide-react'
import { Input } from '../../components/Input'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { getErrorMessage, resolveApiUrl } from '../../lib/apiBaseQuery'
import { useCreateDiscountCodeMutation, useUpdateDiscountCodeMutation, useUploadDiscountCodeImageMutation } from './api'
import type { DiscountCode } from './types'

interface DiscountCodeFormProps {
  /** Omit to add a new discount code; pass an existing one to edit it. */
  discountCode?: DiscountCode
  onDone: () => void
}

export function DiscountCodeForm({ discountCode, onDone }: DiscountCodeFormProps) {
  const [storeName, setStoreName] = useState(discountCode?.storeName ?? '')
  const [codeText, setCodeText] = useState(discountCode?.codeText ?? '')
  const [expiryDate, setExpiryDate] = useState(discountCode?.expiryDate ?? '')
  const [imageFile, setImageFile] = useState<File | null>(null)
  const [imagePreview, setImagePreview] = useState<string | null>(
    discountCode?.codeImageUrl ? resolveApiUrl(discountCode.codeImageUrl) : null,
  )

  const [createDiscountCode, { isLoading: isCreating, error: createError }] = useCreateDiscountCodeMutation()
  const [updateDiscountCode, { isLoading: isUpdating, error: updateError }] = useUpdateDiscountCodeMutation()
  const [uploadDiscountCodeImage, { isLoading: isUploadingImage, error: uploadImageError }] =
    useUploadDiscountCodeImageMutation()

  const isSaving = isCreating || isUpdating || isUploadingImage
  const errorMessage = getErrorMessage(createError ?? updateError ?? uploadImageError)

  function handleImageChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0] ?? null
    if (imageFile) {
      URL.revokeObjectURL(imagePreview!)
    }
    setImageFile(file)
    setImagePreview(
      file ? URL.createObjectURL(file) : discountCode?.codeImageUrl ? resolveApiUrl(discountCode.codeImageUrl) : null,
    )
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const body = {
      storeName,
      codeText: codeText || null,
      expiryDate,
    }
    try {
      let id = discountCode?.id
      if (discountCode) {
        await updateDiscountCode({ id: discountCode.id, ...body }).unwrap()
      } else {
        id = await createDiscountCode(body).unwrap()
      }
      if (imageFile && id) {
        await uploadDiscountCodeImage({ id, file: imageFile }).unwrap()
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
      <div className="flex items-center gap-3">
        {imagePreview ? (
          <img src={imagePreview} alt="" className="h-12 w-12 rounded-lg object-cover" />
        ) : (
          <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-page text-ink-faint">
            <Ticket size={18} />
          </div>
        )}
        <Input label="Code image (optional)" type="file" accept="image/*" onChange={handleImageChange} className="flex-1" />
      </div>
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
