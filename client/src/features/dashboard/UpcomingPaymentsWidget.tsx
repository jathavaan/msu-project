import { Link } from 'react-router-dom'
import { Card } from '../../components/Card'
import { Spinner } from '../../components/Spinner'
import { UpcomingPaymentsList } from '../upcoming-payments/UpcomingPaymentsList'
import { useGetUpcomingPaymentsQuery } from '../upcoming-payments/api'

export function UpcomingPaymentsWidget() {
  const { data: payments, isLoading } = useGetUpcomingPaymentsQuery({ days: 7 })

  return (
    <Card
      title="Next 7 Days"
      actions={
        <Link to="/upcoming-payments" className="text-xs font-medium text-sidebar-active hover:underline">
          View all
        </Link>
      }
    >
      {isLoading ? (
        <Spinner />
      ) : payments && payments.length > 0 ? (
        <UpcomingPaymentsList payments={payments.slice(0, 5)} />
      ) : (
        <p className="py-4 text-center text-sm text-ink-muted">Nothing due in the next 7 days.</p>
      )}
    </Card>
  )
}
