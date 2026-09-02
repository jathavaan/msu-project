import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../components/Input'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { CategoryPicker } from '../categories/CategoryPicker'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { CategoryType } from '../../lib/types'
import { useCreateBudgetMutation, useUpdateBudgetMutation } from './api'
import type { Budget } from './types'

interface BudgetFormProps {
  /** Omit to create a new budget; pass an existing one to edit its limit (category is fixed). */
  budget?: Budget
  onDone: () => void
}

export function BudgetForm({ budget, onDone }: BudgetFormProps) {
  const [categoryId, setCategoryId] = useState(budget?.categoryId ?? '')
  const [monthlyLimit, setMonthlyLimit] = useState(budget?.monthlyLimit.toString() ?? '')

  const [createBudget, { isLoading: isCreating, error: createError }] = useCreateBudgetMutation()
  const [updateBudget, { isLoading: isUpdating, error: updateError }] = useUpdateBudgetMutation()

  const isSaving = isCreating || isUpdating
  const errorMessage = getErrorMessage(createError ?? updateError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    try {
      if (budget) {
        await updateBudget({ id: budget.id, monthlyLimit: Number(monthlyLimit) }).unwrap()
      } else {
        await createBudget({ categoryId, monthlyLimit: Number(monthlyLimit) }).unwrap()
      }
      onDone()
    } catch {
      // Surfaced via the mutation's `error` state below.
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      {budget ? (
        <p className="text-sm text-ink-muted">
          Category: <span className="font-medium text-ink">{budget.categoryName}</span>
        </p>
      ) : (
        <CategoryPicker type={CategoryType.Expense} value={categoryId} onChange={setCategoryId} />
      )}
      <Input
        label="Monthly limit"
        type="number"
        min="0.01"
        step="0.01"
        value={monthlyLimit}
        onChange={(event) => setMonthlyLimit(event.target.value)}
        required
        autoFocus={Boolean(budget)}
      />
      {errorMessage && <ErrorBanner message={errorMessage} />}
      <div className="flex justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onDone}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSaving}>
          {budget ? 'Save changes' : 'Create budget'}
        </Button>
      </div>
    </form>
  )
}
