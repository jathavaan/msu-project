import { Pencil, Trash2 } from 'lucide-react'
import { Button } from '../../components/Button'
import { CategoryBadge } from '../categories/CategoryBadge'
import { formatCurrency, formatRecurrenceDay } from '../../lib/formatters'
import type { Expense } from './types'

interface ExpenseTableProps {
  expenses: Expense[]
  onEdit: (expense: Expense) => void
  onDelete: (expense: Expense) => void
}

export function ExpenseTable({ expenses, onEdit, onDelete }: ExpenseTableProps) {
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
        {expenses.map((expense) => (
          <tr key={expense.id}>
            <td className="py-3 font-medium text-ink">{expense.name}</td>
            <td className="py-3">
              <CategoryBadge category={{ id: expense.categoryId, name: expense.categoryName, type: 'Expense' }} />
            </td>
            <td className="py-3 text-ink-muted">{formatRecurrenceDay(expense.recurrenceDay)}</td>
            <td className="py-3 text-right font-medium text-negative">-{formatCurrency(expense.amount)}</td>
            <td className="py-3">
              <div className="flex justify-end gap-1">
                <Button variant="ghost" size="sm" onClick={() => onEdit(expense)}>
                  <Pencil size={14} />
                </Button>
                <Button variant="ghost" size="sm" onClick={() => onDelete(expense)}>
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
