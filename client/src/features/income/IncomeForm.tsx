import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../components/Input'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { CategoryPicker } from '../categories/CategoryPicker'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { CategoryType } from '../../lib/types'
import { useCreateIncomeMutation, useUpdateIncomeMutation } from './api'
import type { Income } from './types'

interface IncomeFormProps {
  /** Omit to create a new income source; pass an existing one to edit it. */
  income?: Income
  onDone: () => void
}

export function IncomeForm({ income, onDone }: IncomeFormProps) {
  const [name, setName] = useState(income?.name ?? '')
  const [amount, setAmount] = useState(income?.amount.toString() ?? '')
  const [categoryId, setCategoryId] = useState(income?.categoryId ?? '')
  const [recurrenceDay, setRecurrenceDay] = useState(income?.recurrenceDay.toString() ?? '1')

  const [createIncome, { isLoading: isCreating, error: createError }] = useCreateIncomeMutation()
  const [updateIncome, { isLoading: isUpdating, error: updateError }] = useUpdateIncomeMutation()

  const isSaving = isCreating || isUpdating
  const errorMessage = getErrorMessage(createError ?? updateError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const body = { name, amount: Number(amount), categoryId, recurrenceDay: Number(recurrenceDay) }
    try {
      if (income) {
        await updateIncome({ id: income.id, ...body }).unwrap()
      } else {
        await createIncome(body).unwrap()
      }
      onDone()
    } catch {
      // Surfaced via the mutation's `error` state below.
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <Input label="Name" value={name} onChange={(event) => setName(event.target.value)} required autoFocus />
      <Input
        label="Amount"
        type="number"
        min="0.01"
        step="0.01"
        value={amount}
        onChange={(event) => setAmount(event.target.value)}
        required
      />
      <CategoryPicker type={CategoryType.Income} value={categoryId} onChange={setCategoryId} />
      <Input
        label="Recurs on day of month"
        type="number"
        min="1"
        max="28"
        value={recurrenceDay}
        onChange={(event) => setRecurrenceDay(event.target.value)}
        required
      />
      {errorMessage && <ErrorBanner message={errorMessage} />}
      <div className="flex justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onDone}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSaving}>
          {income ? 'Save changes' : 'Add income'}
        </Button>
      </div>
    </form>
  )
}
