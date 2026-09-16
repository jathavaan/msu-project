import { CartesianGrid, Line, LineChart, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Spinner } from '../../components/Spinner'
import { formatCurrency, formatDate } from '../../lib/formatters'
import { POSITIVE_BALANCE_COLOR } from '../../lib/balanceZone'
import { useGetSavingGoalProjectionQuery } from './api'
import { formatReachDateMessage } from './formatReachDate'

interface SavingGoalProjectionChartProps {
  savingGoalId: string
  targetAmount: number
}

// Rounded gridline values on a savings chart don't need cents.
const axisCurrencyFormatter = new Intl.NumberFormat('nb-NO', { style: 'currency', currency: 'NOK', maximumFractionDigits: 0 })

export function SavingGoalProjectionChart({ savingGoalId, targetAmount }: SavingGoalProjectionChartProps) {
  const { data: projection, isLoading } = useGetSavingGoalProjectionQuery(savingGoalId)

  if (isLoading) {
    return <Spinner />
  }

  if (!projection || projection.points.length === 0) {
    return null
  }

  // The line never needs to dip below 0 (a saving goal's balance doesn't go negative), so the
  // domain only has to stretch to cover whichever is taller: the target reference line or the
  // projected balance itself.
  const yMax = Math.max(targetAmount, ...projection.points.map((point) => point.balance)) * 1.05

  return (
    <div>
      <p className="mb-2 text-xs text-ink-muted">{formatReachDateMessage(projection.reachDate)}</p>
      <ResponsiveContainer width="100%" height={200}>
        <LineChart data={projection.points} margin={{ left: 8, right: 8, top: 8, bottom: 8 }}>
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
          <Tooltip
            formatter={(value) => formatCurrency(Number(value))}
            labelFormatter={(label) => formatDate(String(label))}
          />
          <ReferenceLine
            y={targetAmount}
            stroke="#9ca3af"
            strokeDasharray="3 3"
            label={{ value: 'Target', position: 'insideTopRight', fontSize: 11, fill: '#6b7280' }}
          />
          <Line type="monotone" dataKey="balance" stroke={POSITIVE_BALANCE_COLOR} strokeWidth={2} dot={false} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  )
}
