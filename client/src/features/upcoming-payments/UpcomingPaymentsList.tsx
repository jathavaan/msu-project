import { formatCurrency, formatDate } from '../../lib/formatters'
import type { UpcomingPayment } from './types'

export function UpcomingPaymentsList({ payments }: { payments: UpcomingPayment[] }) {
  return (
    <ul className="divide-y divide-border">
      {payments.map((payment) => (
        <li key={`${payment.date}-${payment.name}`} className="flex items-center justify-between py-3 text-sm">
          <div>
            <p className="font-medium text-ink">{payment.name}</p>
            <p className="text-xs text-ink-muted">{formatDate(payment.date)}</p>
          </div>
          <span className={`font-medium ${payment.type === 'Income' ? 'text-positive' : 'text-negative'}`}>
            {payment.type === 'Income' ? '+' : '-'}
            {formatCurrency(payment.amount)}
          </span>
        </li>
      ))}
    </ul>
  )
}
