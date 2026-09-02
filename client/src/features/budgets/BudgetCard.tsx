import { Pencil, Trash2 } from 'lucide-react'
import { Button } from '../../components/Button'
import { ProgressBar } from '../../components/ProgressBar'
import { formatCurrency } from '../../lib/formatters'
import type { Budget } from './types'

interface BudgetCardProps {
  budget: Budget
  onEdit: (budget: Budget) => void
  onDelete: (budget: Budget) => void
}

export function BudgetCard({ budget, onEdit, onDelete }: BudgetCardProps) {
  const remaining = budget.monthlyLimit - budget.usedThisMonth

  return (
    <div className="rounded-xl border border-border p-4">
      <div className="mb-2 flex items-start justify-between">
        <div>
          <p className="text-sm font-semibold text-ink">{budget.categoryName}</p>
          <p className="text-xs text-ink-muted">
            {formatCurrency(budget.usedThisMonth)} of {formatCurrency(budget.monthlyLimit)}
          </p>
        </div>
        <div className="flex gap-1">
          <Button variant="ghost" size="sm" onClick={() => onEdit(budget)}>
            <Pencil size={14} />
          </Button>
          <Button variant="ghost" size="sm" onClick={() => onDelete(budget)}>
            <Trash2 size={14} />
          </Button>
        </div>
      </div>
      <ProgressBar value={budget.usedThisMonth} max={budget.monthlyLimit} />
      <p className={`mt-2 text-xs ${remaining < 0 ? 'text-negative' : 'text-ink-muted'}`}>
        {remaining < 0
          ? `${formatCurrency(Math.abs(remaining))} over budget`
          : `${formatCurrency(remaining)} left this month`}
      </p>
    </div>
  )
}
