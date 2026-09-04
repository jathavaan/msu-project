import { Select } from '../../components/Select'
import { useGetSavingGoalsQuery } from './api'

interface SavingGoalPickerProps {
  value: string
  onChange: (savingGoalId: string) => void
  error?: string
}

/** Saving goal dropdown, shared by any form that links a record to a SavingGoal (e.g. Monthly Savings). */
export function SavingGoalPicker({ value, onChange, error }: SavingGoalPickerProps) {
  const { data: savingGoals, isLoading } = useGetSavingGoalsQuery()

  return (
    <Select
      label="Saving Goal"
      value={value}
      onChange={(event) => onChange(event.target.value)}
      options={(savingGoals ?? []).map((goal) => ({ value: goal.id, label: goal.name }))}
      placeholder={isLoading ? 'Loading saving goals…' : 'Select a saving goal'}
      error={error}
      disabled={isLoading}
      required
    />
  )
}
