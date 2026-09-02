import { Pencil, Trash2 } from 'lucide-react'
import { Button } from '../../components/Button'
import { CategoryBadge } from '../categories/CategoryBadge'
import { formatCurrency, formatRecurrenceDay } from '../../lib/formatters'
import { CategoryType } from '../../lib/types'
import type { Income } from './types'

interface IncomeTableProps {
  incomes: Income[]
  onEdit: (income: Income) => void
  onDelete: (income: Income) => void
}

export function IncomeTable({ incomes, onEdit, onDelete }: IncomeTableProps) {
  return (
    <table className="w-full text-left text-sm">
      <thead>
        <tr className="text-xs text-ink-muted">
          <th className="pb-2 font-medium">Name</th>
          <th className="pb-2 font-medium">Category</th>
          <th className="pb-2 font-medium">Recurs</th>
          <th className="pb-2 text-right font-medium">Amount</th>
          <th className="pb-2" />
        </tr>
      </thead>
      <tbody className="divide-y divide-border">
        {incomes.map((income) => (
          <tr key={income.id}>
            <td className="py-3 font-medium text-ink">{income.name}</td>
            <td className="py-3">
              <CategoryBadge category={{ id: income.categoryId, name: income.categoryName, type: CategoryType.Income }} />
            </td>
            <td className="py-3 text-ink-muted">{formatRecurrenceDay(income.recurrenceDay)}</td>
            <td className="py-3 text-right font-medium text-positive">+{formatCurrency(income.amount)}</td>
            <td className="py-3">
              <div className="flex justify-end gap-1">
                <Button variant="ghost" size="sm" onClick={() => onEdit(income)}>
                  <Pencil size={14} />
                </Button>
                <Button variant="ghost" size="sm" onClick={() => onDelete(income)}>
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
