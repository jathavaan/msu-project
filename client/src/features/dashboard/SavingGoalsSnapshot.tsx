import { Link } from 'react-router-dom'
import { Card } from '../../components/Card'
import { Spinner } from '../../components/Spinner'
import { ProgressBar } from '../../components/ProgressBar'
import { formatCurrency } from '../../lib/formatters'
import { useGetSavingGoalsQuery } from '../saving-goals/api'

export function SavingGoalsSnapshot() {
  const { data: savingGoals, isLoading } = useGetSavingGoalsQuery()

  return (
    <Card
      title="Saving Goals"
      actions={
        <Link to="/saving-goals" className="text-xs font-medium text-sidebar-active hover:underline">
          View all
        </Link>
      }
    >
      {isLoading ? (
        <Spinner />
      ) : savingGoals && savingGoals.length > 0 ? (
        <div className="flex flex-col gap-3">
          {savingGoals.slice(0, 4).map((goal) => (
            <div key={goal.id}>
              <div className="mb-1 flex justify-between text-xs">
                <span className="font-medium text-ink">{goal.name}</span>
                <span className="text-ink-muted">
                  {formatCurrency(goal.amountSaved)} / {formatCurrency(goal.targetAmount)}
                </span>
              </div>
              <ProgressBar value={goal.amountSaved} max={goal.targetAmount} tone="positive" />
            </div>
          ))}
        </div>
      ) : (
        <p className="py-4 text-center text-sm text-ink-muted">No saving goals yet.</p>
      )}
    </Card>
  )
}
