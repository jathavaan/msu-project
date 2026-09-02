import { Link } from 'react-router-dom'
import { Card } from '../../components/Card'
import { Spinner } from '../../components/Spinner'
import { ProgressBar } from '../../components/ProgressBar'
import { formatCurrency } from '../../lib/formatters'
import { useGetBudgetsQuery } from '../budgets/api'

export function BudgetSnapshot() {
  const { data: budgets, isLoading } = useGetBudgetsQuery()

  return (
    <Card
      title="Budgets"
      actions={
        <Link to="/budgets" className="text-xs font-medium text-sidebar-active hover:underline">
          View all
        </Link>
      }
    >
      {isLoading ? (
        <Spinner />
      ) : budgets && budgets.length > 0 ? (
        <div className="flex flex-col gap-3">
          {budgets.slice(0, 4).map((budget) => (
            <div key={budget.id}>
              <div className="mb-1 flex justify-between text-xs">
                <span className="font-medium text-ink">{budget.categoryName}</span>
                <span className="text-ink-muted">
                  {formatCurrency(budget.usedThisMonth)} / {formatCurrency(budget.monthlyLimit)}
                </span>
              </div>
              <ProgressBar value={budget.usedThisMonth} max={budget.monthlyLimit} />
            </div>
          ))}
        </div>
      ) : (
        <p className="py-4 text-center text-sm text-ink-muted">No budgets set yet.</p>
      )}
    </Card>
  )
}
