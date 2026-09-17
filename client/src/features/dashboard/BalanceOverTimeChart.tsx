import { Area, AreaChart, CartesianGrid, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { TooltipContentProps } from 'recharts'
import { Spinner } from '../../components/Spinner'
import { useGetBalanceForecastQuery } from '../balance-forecast/api'
import type { BalanceEntry, BalanceForecastPoint } from '../balance-forecast/types'
import { formatCurrency } from '../../lib/formatters'
import { NEGATIVE_BALANCE_COLOR, POSITIVE_BALANCE_COLOR, zeroCrossingOffset, zeroFloorDomain } from '../../lib/balanceZone'

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

  const hasEntries = point.incomes.length > 0 || point.expenses.length > 0 || point.savings.length > 0

  return (
    <div className="min-w-56 rounded-lg border border-border bg-surface p-3 text-xs shadow-md">
      <div className="mb-2 flex items-center justify-between gap-4">
        <span className="font-semibold text-ink">Day {point.day}</span>
        <span className="font-semibold text-ink">{formatCurrency(point.balance)}</span>
      </div>
      {!hasEntries && <p className="text-ink-muted">No income, expenses, or savings this day.</p>}
      {point.incomes.length > 0 && (
        <div className="mb-1.5 space-y-0.5">
          <p className="font-medium text-positive">Income</p>
          {point.incomes.map((entry) => (
            <EntryRow key={entry.name} entry={entry} />
          ))}
        </div>
      )}
      {point.expenses.length > 0 && (
        <div className="mb-1.5 space-y-0.5">
          <p className="font-medium text-negative">Expenses</p>
          {point.expenses.map((entry) => (
            <EntryRow key={entry.name} entry={entry} />
          ))}
        </div>
      )}
      {point.savings.length > 0 && (
        <div className="space-y-0.5">
          <p className="font-medium text-negative">Savings</p>
          {point.savings.map((entry) => (
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
    return <p className="py-16 text-center text-sm text-ink-muted">No recurring income, expenses, or savings yet.</p>
  }

  const balances = points.map((point) => point.balance)
  const [yMin, yMax] = zeroFloorDomain(balances)
  // This offset lands exactly on the zero gridline (rather than an approximation) because it's
  // derived from the same domain the axis actually renders.
  const offset = zeroCrossingOffset(yMin, yMax)

  return (
    <ResponsiveContainer width="100%" height={260}>
      <AreaChart data={points} margin={{ left: 8, right: 8, top: 8, bottom: 24 }}>
        <defs>
          <linearGradient id="balanceStroke" x1="0" y1="0" x2="0" y2="1">
            <stop offset={offset} stopColor={POSITIVE_BALANCE_COLOR} />
            <stop offset={offset} stopColor={NEGATIVE_BALANCE_COLOR} />
          </linearGradient>
          <linearGradient id="balanceFill" x1="0" y1="0" x2="0" y2="1">
            <stop offset={offset} stopColor={POSITIVE_BALANCE_COLOR} stopOpacity={0.2} />
            <stop offset={offset} stopColor={NEGATIVE_BALANCE_COLOR} stopOpacity={0.2} />
          </linearGradient>
        </defs>
        <CartesianGrid vertical={false} stroke="#e5e7eb" />
        <XAxis
          dataKey="day"
          tickLine={false}
          axisLine={false}
          tick={{ fill: '#6b7280', fontSize: 12 }}
          label={{ value: 'Day of month', position: 'insideBottom', offset: -4, fontSize: 11, fill: '#6b7280' }}
        />
        <YAxis
          domain={[yMin, yMax]}
          tickLine={false}
          axisLine={false}
          tick={{ fill: '#6b7280', fontSize: 12 }}
          width={92}
          tickFormatter={(value) => axisCurrencyFormatter.format(Number(value))}
        />
        <Tooltip content={BalanceTooltip} />
        {yMin < 0 && yMax > 0 && <ReferenceLine y={0} stroke="#9ca3af" strokeDasharray="3 3" />}
        <Area type="monotone" dataKey="balance" stroke="url(#balanceStroke)" strokeWidth={2} fill="url(#balanceFill)" />
      </AreaChart>
    </ResponsiveContainer>
  )
}
