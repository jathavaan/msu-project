import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { TooltipContentProps } from 'recharts'
import { Spinner } from '../../components/Spinner'
import { useGetBalanceForecastQuery } from '../balance-forecast/api'
import type { BalanceEntry, BalanceForecastPoint } from '../balance-forecast/types'
import { formatCurrency } from '../../lib/formatters'

// The balance can run into six figures (unlike the other dashboard charts, which only ever plot
// a single month's totals), so axis ticks drop the decimals `formatCurrency` always includes —
// full cents on a rounded gridline value add width without adding information.
const axisCurrencyFormatter = new Intl.NumberFormat('nb-NO', { style: 'currency', currency: 'NOK', maximumFractionDigits: 0 })

function EntryRow({ entry }: { entry: BalanceEntry }) {
  return (
    <p className="flex items-baseline justify-between gap-4">
      <span className="truncate text-ink-muted">
        {entry.name} <span className="text-ink-faint">· {entry.categoryName}</span>
      </span>
      <span className="shrink-0 font-medium text-ink">{formatCurrency(entry.amount)}</span>
    </p>
  )
}

// Custom tooltip content (rather than the default `formatter` prop) because the ask is to show
// every income/expense entry applied that day, not just the plotted balance value — recharts'
// default tooltip only ever renders the series' own numeric value.
function BalanceTooltip({ active, payload }: TooltipContentProps) {
  if (!active || !payload?.length) {
    return null
  }

  const point = payload[0]?.payload as BalanceForecastPoint | undefined
  if (!point) {
    return null
  }

  const hasEntries = point.incomes.length > 0 || point.expenses.length > 0

  return (
    <div className="min-w-56 rounded-lg border border-border bg-surface p-3 text-xs shadow-md">
      <div className="mb-2 flex items-center justify-between gap-4">
        <span className="font-semibold text-ink">Day {point.day}</span>
        <span className="font-semibold text-ink">{formatCurrency(point.balance)}</span>
      </div>
      {!hasEntries && <p className="text-ink-muted">No income or expenses this day.</p>}
      {point.incomes.length > 0 && (
        <div className="mb-1.5 space-y-0.5">
          <p className="font-medium text-positive">Income</p>
          {point.incomes.map((entry) => (
            <EntryRow key={entry.name} entry={entry} />
          ))}
        </div>
      )}
      {point.expenses.length > 0 && (
        <div className="space-y-0.5">
          <p className="font-medium text-negative">Expenses</p>
          {point.expenses.map((entry) => (
            <EntryRow key={entry.name} entry={entry} />
          ))}
        </div>
      )}
    </div>
  )
}

export function BalanceOverTimeChart() {
  const { data: points, isLoading } = useGetBalanceForecastQuery()

  if (isLoading) {
    return <Spinner />
  }

  if (!points || points.length === 0) {
    return <p className="py-16 text-center text-sm text-ink-muted">No recurring income or expenses yet.</p>
  }

  return (
    <ResponsiveContainer width="100%" height={260}>
      <AreaChart data={points} margin={{ left: 8, right: 8, top: 8, bottom: 0 }}>
        <defs>
          <linearGradient id="balanceFill" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="#2563eb" stopOpacity={0.25} />
            <stop offset="95%" stopColor="#2563eb" stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid vertical={false} stroke="#e5e7eb" />
        <XAxis
          dataKey="day"
          tickLine={false}
          axisLine={false}
          tick={{ fill: '#6b7280', fontSize: 12 }}
          label={{ value: 'Day of period', position: 'insideBottom', offset: -4, fontSize: 11, fill: '#6b7280' }}
        />
        <YAxis
          tickLine={false}
          axisLine={false}
          tick={{ fill: '#6b7280', fontSize: 12 }}
          width={92}
          tickFormatter={(value) => axisCurrencyFormatter.format(Number(value))}
        />
        <Tooltip content={BalanceTooltip} />
        <Area type="monotone" dataKey="balance" stroke="#2563eb" strokeWidth={2} fill="url(#balanceFill)" />
      </AreaChart>
    </ResponsiveContainer>
  )
}
