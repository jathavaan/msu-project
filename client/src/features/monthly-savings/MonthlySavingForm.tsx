import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../components/Input'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { SavingGoalPicker } from '../saving-goals/SavingGoalPicker'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { useCreateMonthlySavingMutation, useUpdateMonthlySavingMutation } from './api'
import type { MonthlySaving } from './types'

interface MonthlySavingFormProps {
  /** Omit to create a new monthly saving; pass an existing one to edit it. */
  monthlySaving?: MonthlySaving
  onDone: () => void
}

export function MonthlySavingForm({ monthlySaving, onDone }: MonthlySavingFormProps) {
  const [name, setName] = useState(monthlySaving?.name ?? '')
  const [amount, setAmount] = useState(monthlySaving?.amount.toString() ?? '')
  const [savingGoalId, setSavingGoalId] = useState(monthlySaving?.savingGoalId ?? '')
  const [recurrenceDay, setRecurrenceDay] = useState(monthlySaving?.recurrenceDay.toString() ?? '1')

  const [createMonthlySaving, { isLoading: isCreating, error: createError }] = useCreateMonthlySavingMutation()
  const [updateMonthlySaving, { isLoading: isUpdating, error: updateError }] = useUpdateMonthlySavingMutation()

  const isSaving = isCreating || isUpdating
  const errorMessage = getErrorMessage(createError ?? updateError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const body = { name, amount: Number(amount), savingGoalId, recurrenceDay: Number(recurrenceDay) }
    try {
      if (monthlySaving) {
        await updateMonthlySaving({ id: monthlySaving.id, ...body }).unwrap()
      } else {
        await createMonthlySaving(body).unwrap()
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
      <SavingGoalPicker value={savingGoalId} onChange={setSavingGoalId} />
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
          {monthlySaving ? 'Save changes' : 'Add monthly saving'}
        </Button>
      </div>
    </form>
  )
}
