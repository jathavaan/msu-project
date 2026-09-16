import { Pencil, Trash2, Image as ImageIcon } from 'lucide-react'
import { Button } from '../../components/Button'
import { ProgressBar } from '../../components/ProgressBar'
import { resolveApiUrl } from '../../lib/apiBaseQuery'
import { formatCurrency, formatDate } from '../../lib/formatters'
import { useGetSavingGoalProjectionQuery } from './api'
import { formatReachDateMessage } from './formatReachDate'
import type { SavingGoal } from './types'

interface SavingGoalCardProps {
  savingGoal: SavingGoal
  onEdit: (savingGoal: SavingGoal) => void
  onDelete: (savingGoal: SavingGoal) => void
}

export function SavingGoalCard({ savingGoal, onEdit, onDelete }: SavingGoalCardProps) {
  // A goal already met has nothing left to project — skip the request and let the progress bar
  // (already at/over 100%) speak for itself instead.
  const alreadyMet = savingGoal.amountRemaining <= 0
  const { data: projection } = useGetSavingGoalProjectionQuery(savingGoal.id, { skip: alreadyMet })

  return (
    <div className="rounded-xl border border-border p-4">
      <div className="mb-2 flex items-start justify-between">
        <div className="flex items-center gap-2">
          {savingGoal.imageUrl ? (
            <img src={resolveApiUrl(savingGoal.imageUrl)} alt="" className="h-9 w-9 rounded-lg object-cover" />
          ) : (
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-page text-ink-faint">
              <ImageIcon size={16} />
            </div>
          )}
          <div>
            <p className="text-sm font-semibold text-ink">{savingGoal.name}</p>
            <p className="text-xs text-ink-muted">Target: {formatDate(savingGoal.targetDate)}</p>
          </div>
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
      {savingGoal.monthlyContribution > 0 && (
        <p className="mt-2 text-xs text-ink-muted">
          Saving <span className="font-medium text-positive">{formatCurrency(savingGoal.monthlyContribution)}</span>/month towards this goal
        </p>
      )}
      {!alreadyMet && projection && <p className="mt-1 text-xs text-ink-muted">{formatReachDateMessage(projection.reachDate)}</p>}
    </div>
  )
}
