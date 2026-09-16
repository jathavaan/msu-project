import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../components/Input'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { useCreateSavingGoalMutation, useUpdateSavingGoalMutation } from './api'
import { SavingGoalProjectionChart } from './SavingGoalProjectionChart'
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
  const [interestRate, setInterestRate] = useState(savingGoal?.interestRate?.toString() ?? '')

  const [createSavingGoal, { isLoading: isCreating, error: createError }] = useCreateSavingGoalMutation()
  const [updateSavingGoal, { isLoading: isUpdating, error: updateError }] = useUpdateSavingGoalMutation()

  const isSaving = isCreating || isUpdating
  const errorMessage = getErrorMessage(createError ?? updateError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const parsedInterestRate = interestRate ? Number(interestRate) : null
    try {
      if (savingGoal) {
        await updateSavingGoal({
          id: savingGoal.id,
          name,
          targetAmount: Number(targetAmount),
          targetDate,
          currentAmount: Number(amountSaved),
          interestRate: parsedInterestRate,
        }).unwrap()
      } else {
        await createSavingGoal({
          name,
          targetAmount: Number(targetAmount),
          targetDate,
          interestRate: parsedInterestRate,
        }).unwrap()
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
      <Input
        label="Interest rate (annual %, optional)"
        type="number"
        min="0"
        max="100"
        step="0.01"
        placeholder="e.g. 4.5"
        value={interestRate}
        onChange={(event) => setInterestRate(event.target.value)}
      />
      {errorMessage && <ErrorBanner message={errorMessage} />}
      {savingGoal && (
        <div className="border-t border-border pt-4">
          <p className="mb-2 text-xs font-medium text-ink-muted">Projected balance</p>
          <SavingGoalProjectionChart savingGoalId={savingGoal.id} targetAmount={savingGoal.targetAmount} />
        </div>
      )}
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
