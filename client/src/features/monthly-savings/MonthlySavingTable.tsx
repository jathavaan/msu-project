import { Pencil, Trash2 } from 'lucide-react'
import { Button } from '../../components/Button'
import { formatCurrency, formatRecurrenceDay } from '../../lib/formatters'
import type { MonthlySaving } from './types'

interface MonthlySavingTableProps {
  monthlySavings: MonthlySaving[]
  onEdit: (monthlySaving: MonthlySaving) => void
  onDelete: (monthlySaving: MonthlySaving) => void
}

export function MonthlySavingTable({ monthlySavings, onEdit, onDelete }: MonthlySavingTableProps) {
  return (
    <table className="w-full text-left text-sm">
      <thead>
        <tr className="text-xs text-ink-muted">
          <th className="pb-2 font-medium">Name</th>
          <th className="pb-2 font-medium">Saving Goal</th>
          <th className="pb-2 font-medium">Recurs</th>
          <th className="pb-2 text-right font-medium">Amount</th>
          <th className="pb-2" />
        </tr>
      </thead>
      <tbody className="divide-y divide-border">
        {monthlySavings.map((monthlySaving) => (
          <tr key={monthlySaving.id}>
            <td className="py-3 font-medium text-ink">{monthlySaving.name}</td>
            <td className="py-3 text-ink-muted">{monthlySaving.savingGoalName}</td>
            <td className="py-3 text-ink-muted">{formatRecurrenceDay(monthlySaving.recurrenceDay)}</td>
            <td className="py-3 text-right font-medium text-positive">+{formatCurrency(monthlySaving.amount)}</td>
            <td className="py-3">
              <div className="flex justify-end gap-1">
                <Button variant="ghost" size="sm" onClick={() => onEdit(monthlySaving)}>
                  <Pencil size={14} />
                </Button>
                <Button variant="ghost" size="sm" onClick={() => onDelete(monthlySaving)}>
                  <Trash2 size={14} />
                </Button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
