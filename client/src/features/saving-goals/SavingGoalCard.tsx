import { Pencil, Trash2 } from 'lucide-react'
import { Button } from '../../components/Button'
import { ProgressBar } from '../../components/ProgressBar'
import { formatCurrency, formatDate } from '../../lib/formatters'
import type { SavingGoal } from './types'

interface SavingGoalCardProps {
  savingGoal: SavingGoal
  onEdit: (savingGoal: SavingGoal) => void
  onDelete: (savingGoal: SavingGoal) => void
}

export function SavingGoalCard({ savingGoal, onEdit, onDelete }: SavingGoalCardProps) {
  return (
    <div className="rounded-xl border border-border p-4">
      <div className="mb-2 flex items-start justify-between">
        <div>
          <p className="text-sm font-semibold text-ink">{savingGoal.name}</p>
          <p className="text-xs text-ink-muted">Target: {formatDate(savingGoal.targetDate)}</p>
        </div>
        <div className="flex gap-1">
          <Button variant="ghost" size="sm" onClick={() => onEdit(savingGoal)}>
            <Pencil size={14} />
          </Button>
          <Button variant="ghost" size="sm" onClick={() => onDelete(savingGoal)}>
            <Trash2 size={14} />
          </Button>
        </div>
      </div>
      <ProgressBar value={savingGoal.amountSaved} max={savingGoal.targetAmount} tone="positive" />
      <div className="mt-2 flex items-center justify-between text-xs text-ink-muted">
        <span>
          {formatCurrency(savingGoal.amountSaved)} of {formatCurrency(savingGoal.targetAmount)}
        </span>
        <span>
          {savingGoal.daysRemaining >= 0 ? `${savingGoal.daysRemaining} days left` : `${Math.abs(savingGoal.daysRemaining)} days overdue`}
        </span>
      </div>
    </div>
  )
}
