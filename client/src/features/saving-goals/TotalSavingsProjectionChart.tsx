import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Spinner } from '../../components/Spinner'
import { formatCurrency, formatDate } from '../../lib/formatters'
import { POSITIVE_BALANCE_COLOR } from '../../lib/balanceZone'
import { useGetSavingGoalsProjectionQuery } from './api'

// Rounded gridline values on a savings chart don't need cents.
const axisCurrencyFormatter = new Intl.NumberFormat('nb-NO', { style: 'currency', currency: 'NOK', maximumFractionDigits: 0 })

interface TotalSavingsProjectionChartProps {
  years: number
}

export function TotalSavingsProjectionChart({ years }: TotalSavingsProjectionChartProps) {
  const { data: points, isLoading } = useGetSavingGoalsProjectionQuery({ years })

  if (isLoading) {
    return <Spinner />
  }

  if (!points || points.length === 0) {
    return null
  }

  // The combined balance never goes negative (no individual goal balance does, and a goal past its
  // own target just keeps growing rather than being clamped), so the domain only needs to stretch
  // to cover the highest projected total. Math.max(1, ...) keeps the domain non-degenerate if every
  // goal is still at 0.
  const yMax = Math.max(1, ...points.map((point) => point.totalBalance)) * 1.05

  return (
    <ResponsiveContainer width="100%" height={220}>
      <LineChart data={points} margin={{ left: 8, right: 8, top: 8, bottom: 8 }}>
        <CartesianGrid vertical={false} stroke="#e5e7eb" />
        <XAxis
          dataKey="date"
          tickFormatter={(value: string) => formatDate(value)}
          tickLine={false}
          axisLine={false}
          tick={{ fill: '#6b7280', fontSize: 11 }}
        />
        <YAxis
          domain={[0, yMax]}
          tickLine={false}
          axisLine={false}
          tick={{ fill: '#6b7280', fontSize: 12 }}
          width={80}
          tickFormatter={(value) => axisCurrencyFormatter.format(Number(value))}
        />
        <Tooltip formatter={(value) => formatCurrency(Number(value))} labelFormatter={(label) => formatDate(String(label))} />
        <Line type="monotone" dataKey="totalBalance" stroke={POSITIVE_BALANCE_COLOR} strokeWidth={2} dot={false} />
      </LineChart>
    </ResponsiveContainer>
  )
}
