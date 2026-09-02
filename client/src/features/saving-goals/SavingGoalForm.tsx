import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../components/Input'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { useCreateSavingGoalMutation, useUpdateSavingGoalMutation } from './api'
import type { SavingGoal } from './types'

interface SavingGoalFormProps {
  /** Omit to create a new goal; pass an existing one to edit it (and adjust the saved amount). */
  savingGoal?: SavingGoal
  onDone: () => void
}

export function SavingGoalForm({ savingGoal, onDone }: SavingGoalFormProps) {
  const [name, setName] = useState(savingGoal?.name ?? '')
  const [targetAmount, setTargetAmount] = useState(savingGoal?.targetAmount.toString() ?? '')
  const [targetDate, setTargetDate] = useState(savingGoal?.targetDate ?? '')
  const [amountSaved, setAmountSaved] = useState(savingGoal?.amountSaved.toString() ?? '0')

  const [createSavingGoal, { isLoading: isCreating, error: createError }] = useCreateSavingGoalMutation()
  const [updateSavingGoal, { isLoading: isUpdating, error: updateError }] = useUpdateSavingGoalMutation()

  const isSaving = isCreating || isUpdating
  const errorMessage = getErrorMessage(createError ?? updateError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    try {
      if (savingGoal) {
        await updateSavingGoal({
          id: savingGoal.id,
          name,
          targetAmount: Number(targetAmount),
          targetDate,
          currentAmount: Number(amountSaved),
        }).unwrap()
      } else {
        await createSavingGoal({ name, targetAmount: Number(targetAmount), targetDate }).unwrap()
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
        label="Target amount"
        type="number"
        min="0.01"
        step="0.01"
        value={targetAmount}
        onChange={(event) => setTargetAmount(event.target.value)}
        required
      />
      <Input
        label="Target date"
        type="date"
        value={targetDate}
        onChange={(event) => setTargetDate(event.target.value)}
        required
      />
      {savingGoal && (
        <Input
          label="Amount saved so far"
          type="number"
          min="0"
          step="0.01"
          value={amountSaved}
          onChange={(event) => setAmountSaved(event.target.value)}
          required
        />
      )}
      {errorMessage && <ErrorBanner message={errorMessage} />}
      <div className="flex justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onDone}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSaving}>
          {savingGoal ? 'Save changes' : 'Create goal'}
        </Button>
      </div>
    </form>
  )
}
