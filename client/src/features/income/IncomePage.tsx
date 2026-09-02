import { useState } from 'react'
import { Plus, TrendingUp } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Card } from '../../components/Card'
import { Button } from '../../components/Button'
import { Modal } from '../../components/Modal'
import { QueryState } from '../../components/QueryState'
import { EmptyState } from '../../components/EmptyState'
import { IncomeTable } from './IncomeTable'
import { IncomeForm } from './IncomeForm'
import { useDeleteIncomeMutation, useGetIncomesQuery } from './api'
import type { Income } from './types'

export function IncomePage() {
  const { data: incomes, isLoading, error } = useGetIncomesQuery()
  const [deleteIncome] = useDeleteIncomeMutation()
  const [editing, setEditing] = useState<Income | 'new' | null>(null)

  function handleDelete(income: Income) {
    if (window.confirm(`Delete "${income.name}"?`)) {
      void deleteIncome(income.id)
    }
  }

  return (
    <>
      <PageHeader
        title="Income"
        description="Known, recurring income like salary or other regular payments."
        actions={
          <Button icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Add Income
          </Button>
        }
      />

      <Card>
        <QueryState
          isLoading={isLoading}
          error={error}
          isEmpty={incomes?.length === 0}
          empty={
            <EmptyState
              icon={<TrendingUp size={24} />}
              title="No income sources yet"
              description="Add a recurring income source, like your salary, to get started."
            />
          }
        >
          <IncomeTable incomes={incomes ?? []} onEdit={setEditing} onDelete={handleDelete} />
        </QueryState>
      </Card>

      <Modal open={editing !== null} onClose={() => setEditing(null)} title={editing === 'new' ? 'Add Income' : 'Edit Income'}>
        {editing !== null && <IncomeForm income={editing === 'new' ? undefined : editing} onDone={() => setEditing(null)} />}
      </Modal>
    </>
  )
}
