import { useState } from 'react'
import { CalendarClock } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Card } from '../../components/Card'
import { Select } from '../../components/Select'
import { QueryState } from '../../components/QueryState'
import { EmptyState } from '../../components/EmptyState'
import { UpcomingPaymentsList } from './UpcomingPaymentsList'
import { useGetUpcomingPaymentsQuery } from './api'

const RANGE_OPTIONS = [
  { value: '7', label: 'Next 7 days' },
  { value: '14', label: 'Next 14 days' },
  { value: '30', label: 'Next 30 days' },
]

export function UpcomingPaymentsPage() {
  const [days, setDays] = useState('7')
  const { data: payments, isLoading, error } = useGetUpcomingPaymentsQuery({ days: Number(days) })

  return (
    <>
      <PageHeader title="Upcoming Payments" description="Income and expenses due soon, based on their recurrence day." />

      <Card
        title="Payments"
        actions={
          <Select label="Range" value={days} onChange={(event) => setDays(event.target.value)} options={RANGE_OPTIONS} className="w-40" />
        }
      >
        <QueryState
          isLoading={isLoading}
          error={error}
          isEmpty={payments?.length === 0}
          empty={<EmptyState icon={<CalendarClock size={24} />} title="Nothing due" description="No income or expenses fall in this range." />}
        >
          <UpcomingPaymentsList payments={payments ?? []} />
        </QueryState>
      </Card>
    </>
  )
}
