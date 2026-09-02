import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../components/Input'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { CategoryPicker } from '../categories/CategoryPicker'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { CategoryType } from '../../lib/types'
import { useCreateExpenseMutation, useUpdateExpenseMutation } from './api'
import type { Expense } from './types'

interface ExpenseFormProps {
  /** Omit to create a new expense; pass an existing one to edit it. */
  expense?: Expense
  onDone: () => void
}

export function ExpenseForm({ expense, onDone }: ExpenseFormProps) {
  const [name, setName] = useState(expense?.name ?? '')
  const [amount, setAmount] = useState(expense?.amount.toString() ?? '')
  const [categoryId, setCategoryId] = useState(expense?.categoryId ?? '')
  const [recurrenceDay, setRecurrenceDay] = useState(expense?.recurrenceDay.toString() ?? '1')

  const [createExpense, { isLoading: isCreating, error: createError }] = useCreateExpenseMutation()
  const [updateExpense, { isLoading: isUpdating, error: updateError }] = useUpdateExpenseMutation()

  const isSaving = isCreating || isUpdating
  const errorMessage = getErrorMessage(createError ?? updateError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const body = { name, amount: Number(amount), categoryId, recurrenceDay: Number(recurrenceDay) }
    try {
      if (expense) {
        await updateExpense({ id: expense.id, ...body }).unwrap()
      } else {
        await createExpense(body).unwrap()
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
      <CategoryPicker type={CategoryType.Expense} value={categoryId} onChange={setCategoryId} />
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
          {expense ? 'Save changes' : 'Add expense'}
        </Button>
      </div>
    </form>
  )
}
