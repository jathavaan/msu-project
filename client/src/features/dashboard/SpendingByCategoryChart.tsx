import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts'
import { categoryHex } from '../../lib/categoryColor'
import { formatCurrency } from '../../lib/formatters'
import type { Expense } from '../expenses/types'

export function SpendingByCategoryChart({ expenses }: { expenses: Expense[] }) {
  const totals = new Map<string, { name: string; value: number }>()
  for (const expense of expenses) {
    const existing = totals.get(expense.categoryId)
    if (existing) {
      existing.value += expense.amount
    } else {
      totals.set(expense.categoryId, { name: expense.categoryName, value: expense.amount })
    }
  }
  const data = Array.from(totals.entries()).map(([categoryId, entry]) => ({ categoryId, ...entry }))

  if (data.length === 0) {
    return <p className="py-16 text-center text-sm text-ink-muted">No expenses yet.</p>
  }

  return (
    <ResponsiveContainer width="100%" height={220}>
      <PieChart>
        <Pie data={data} dataKey="value" nameKey="name" innerRadius={50} outerRadius={80} paddingAngle={2}>
          {data.map((entry) => (
            <Cell key={entry.categoryId} fill={categoryHex(entry.categoryId)} />
          ))}
        </Pie>
        <Tooltip formatter={(value) => formatCurrency(Number(value))} />
      </PieChart>
    </ResponsiveContainer>
  )
}
