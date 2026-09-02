import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../components/Input'
import { Select } from '../../components/Select'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { useCreateCategoryMutation, useUpdateCategoryMutation } from './api'
import type { Category } from './types'
import { CategoryType } from '../../lib/types'

// Select values are always strings on the DOM; converted to/from CategoryType at the form boundary.
const TYPE_OPTIONS = [
  { value: String(CategoryType.Income), label: 'Income' },
  { value: String(CategoryType.Expense), label: 'Expense' },
]

interface CategoryFormProps {
  /** Omit to create a new category; pass an existing one to edit it. */
  category?: Category
  onDone: () => void
}

export function CategoryForm({ category, onDone }: CategoryFormProps) {
  const [name, setName] = useState(category?.name ?? '')
  const [type, setType] = useState(String(category?.type ?? CategoryType.Expense))
  const [createCategory, { isLoading: isCreating, error: createError }] = useCreateCategoryMutation()
  const [updateCategory, { isLoading: isUpdating, error: updateError }] = useUpdateCategoryMutation()

  const isSaving = isCreating || isUpdating
  const errorMessage = getErrorMessage(createError ?? updateError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const body = { name, type: Number(type) as CategoryType }
    try {
      if (category) {
        await updateCategory({ id: category.id, ...body }).unwrap()
      } else {
        await createCategory(body).unwrap()
      }
      onDone()
    } catch {
      // Surfaced via the mutation's `error` state below.
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <Input label="Name" value={name} onChange={(event) => setName(event.target.value)} required autoFocus />
      <Select
        label="Type"
        value={type}
        onChange={(event) => setType(event.target.value)}
        options={TYPE_OPTIONS}
      />
      {errorMessage && <ErrorBanner message={errorMessage} />}
      <div className="flex justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onDone}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSaving}>
          {category ? 'Save changes' : 'Create category'}
        </Button>
      </div>
    </form>
  )
}
