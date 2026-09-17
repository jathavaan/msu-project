import { Bar, BarChart, CartesianGrid, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Spinner } from '../../components/Spinner'
import { NEGATIVE_BALANCE_COLOR } from '../../lib/balanceZone'
import { formatCurrency, formatMonthLabel } from '../../lib/formatters'
import { useGetCategorySpendTrendQuery } from './api'

interface SpendTrendChartProps {
  categoryId: string
}

// Rounded gridline values don't need cents.
const axisCurrencyFormatter = new Intl.NumberFormat('nb-NO', { style: 'currency', currency: 'NOK', maximumFractionDigits: 0 })

export function SpendTrendChart({ categoryId }: SpendTrendChartProps) {
  const { data: trend, isLoading } = useGetCategorySpendTrendQuery(categoryId)

  if (isLoading) {
    return <Spinner />
  }

  if (!trend || trend.months.length === 0) {
    return null
  }

  const data = trend.months.map((month) => ({ ...month, label: formatMonthLabel(month.year, month.month) }))

  return (
    <ResponsiveContainer width="100%" height={260}>
      <BarChart data={data} margin={{ left: 8, right: 8, top: 8, bottom: 8 }}>
        <CartesianGrid vertical={false} stroke="#e5e7eb" />
        <XAxis dataKey="label" tickLine={false} axisLine={false} tick={{ fill: '#6b7280', fontSize: 12 }} />
        <YAxis
          tickLine={false}
          axisLine={false}
          tick={{ fill: '#6b7280', fontSize: 12 }}
          width={80}
          tickFormatter={(value) => axisCurrencyFormatter.format(Number(value))}
        />
        <Tooltip formatter={(value) => formatCurrency(Number(value))} />
        {trend.monthlyLimit != null && (
          <ReferenceLine
            y={trend.monthlyLimit}
            stroke="#9ca3af"
            strokeDasharray="3 3"
            label={{ value: 'Budget', position: 'insideTopRight', fontSize: 11, fill: '#6b7280' }}
          />
        )}
        <Bar dataKey="actual" fill={NEGATIVE_BALANCE_COLOR} radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  )
}
