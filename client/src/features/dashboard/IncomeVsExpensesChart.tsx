import { Bar, BarChart, CartesianGrid, Cell, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { formatCurrency } from '../../lib/formatters'

interface IncomeVsExpensesChartProps {
  totalIncome: number
  totalExpenses: number
}

export function IncomeVsExpensesChart({ totalIncome, totalExpenses }: IncomeVsExpensesChartProps) {
  const data = [
    { name: 'Income', amount: totalIncome, fill: '#16a34a' },
    { name: 'Expenses', amount: totalExpenses, fill: '#dc2626' },
  ]

  return (
    <ResponsiveContainer width="100%" height={220}>
      <BarChart data={data} barSize={56}>
        <CartesianGrid vertical={false} stroke="#e5e7eb" />
        <XAxis dataKey="name" tickLine={false} axisLine={false} tick={{ fill: '#6b7280', fontSize: 12 }} />
        <YAxis tickLine={false} axisLine={false} tick={{ fill: '#6b7280', fontSize: 12 }} width={64} />
        <Tooltip formatter={(value) => formatCurrency(Number(value))} />
        <Bar dataKey="amount" radius={[6, 6, 0, 0]}>
          {data.map((entry) => (
            <Cell key={entry.name} fill={entry.fill} />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  )
}
