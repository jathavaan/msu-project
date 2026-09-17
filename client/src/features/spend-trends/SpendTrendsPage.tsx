import { useState } from 'react'
import { LineChart } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Card } from '../../components/Card'
import { EmptyState } from '../../components/EmptyState'
import { QueryState } from '../../components/QueryState'
import { CategoryPicker } from '../categories/CategoryPicker'
import { SpendTrendChart } from './SpendTrendChart'
import { useGetTransactionsQuery } from '../transactions/api'
import { CategoryType } from '../../lib/types'

export function SpendTrendsPage() {
  const { data: transactions, isLoading, error } = useGetTransactionsQuery()
  const [categoryId, setCategoryId] = useState('')

  return (
    <>
      <PageHeader
        title="Spend Trends"
        description="How spending in a category has trended over the last 6 months, against its budget."
      />

      <QueryState
        isLoading={isLoading}
        error={error}
        isEmpty={transactions?.length === 0}
        empty={
          <EmptyState
            icon={<LineChart size={24} />}
            title="No imported transactions yet"
            description="Import a bank statement to see how your spending in each category is trending. Trends are based on imported transactions, not planned expenses."
          />
        }
      >
        <Card>
          <div className="mb-4 max-w-xs">
            <CategoryPicker type={CategoryType.Expense} value={categoryId} onChange={setCategoryId} />
          </div>
          {categoryId ? (
            <SpendTrendChart categoryId={categoryId} />
          ) : (
            <p className="py-16 text-center text-sm text-ink-muted">Select a category to see its spend trend.</p>
          )}
        </Card>
      </QueryState>
    </>
  )
}
