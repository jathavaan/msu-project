import { Link } from 'react-router-dom'
import { Card } from '../../components/Card'
import { Spinner } from '../../components/Spinner'
import { formatCurrency, formatRecurrenceDay } from '../../lib/formatters'
import { useGetMonthlySavingsQuery } from '../monthly-savings/api'

export function MonthlySavingsSnapshot() {
  const { data: monthlySavings, isLoading } = useGetMonthlySavingsQuery()

  return (
    <Card
      title="Monthly Savings"
      actions={
        <Link to="/monthly-savings" className="text-xs font-medium text-sidebar-active hover:underline">
          View all
        </Link>
      }
    >
      {isLoading ? (
        <Spinner />
      ) : monthlySavings && monthlySavings.length > 0 ? (
        <div className="flex flex-col gap-3">
          {monthlySavings.slice(0, 4).map((saving) => (
            <div key={saving.id} className="flex items-center justify-between text-xs">
              <div>
                <p className="font-medium text-ink">{saving.name}</p>
                <p className="text-ink-muted">
                  {saving.savingGoalName} &middot; {formatRecurrenceDay(saving.recurrenceDay)}
                </p>
              </div>
              <span className="font-medium text-positive">{formatCurrency(saving.amount)}</span>
            </div>
          ))}
        </div>
      ) : (
        <p className="py-4 text-center text-sm text-ink-muted">No monthly savings set up yet.</p>
      )}
    </Card>
  )
}
