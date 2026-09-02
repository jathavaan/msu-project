import { useState } from 'react'
import { Plus, TrendingDown } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Card } from '../../components/Card'
import { Button } from '../../components/Button'
import { Modal } from '../../components/Modal'
import { ConfirmDialog } from '../../components/ConfirmDialog'
import { Select } from '../../components/Select'
import { QueryState } from '../../components/QueryState'
import { EmptyState } from '../../components/EmptyState'
import { ExpenseTable } from './ExpenseTable'
import { ExpenseForm } from './ExpenseForm'
import { useDeleteExpenseMutation, useGetExpensesQuery } from './api'
import { useGetCategoriesQuery } from '../categories/api'
import { CategoryType } from '../../lib/types'
import type { Expense } from './types'

export function ExpensesPage() {
  const [categoryFilter, setCategoryFilter] = useState('')
  const { data: expenseCategories } = useGetCategoriesQuery({ type: CategoryType.Expense })
  const { data: expenses, isLoading, error } = useGetExpensesQuery({ categoryId: categoryFilter || undefined })
  const [deleteExpense] = useDeleteExpenseMutation()
  const [editing, setEditing] = useState<Expense | 'new' | null>(null)
  const [deleting, setDeleting] = useState<Expense | null>(null)

  function handleConfirmDelete() {
    if (deleting) void deleteExpense(deleting.id)
    setDeleting(null)
  }

  return (
    <>
      <PageHeader
        title="Expenses"
        description="Known, recurring expenses like rent, loan payments, or subscriptions."
        actions={
          <Button icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Add Expense
          </Button>
        }
      />

      <Card
        actions={
          <Select
            label="Filter by category"
            value={categoryFilter}
            onChange={(event) => setCategoryFilter(event.target.value)}
            options={[
              { value: '', label: 'All categories' },
              ...(expenseCategories ?? []).map((category) => ({ value: category.id, label: category.name })),
            ]}
            className="w-48"
          />
        }
      >
        <QueryState
          isLoading={isLoading}
          error={error}
          isEmpty={expenses?.length === 0}
          empty={
            <EmptyState
              icon={<TrendingDown size={24} />}
              title="No expenses yet"
              description="Add a recurring expense, like rent or a subscription, to get started."
            />
          }
        >
          <ExpenseTable expenses={expenses ?? []} onEdit={setEditing} onDelete={setDeleting} />
        </QueryState>
      </Card>

      <Modal open={editing !== null} onClose={() => setEditing(null)} title={editing === 'new' ? 'Add Expense' : 'Edit Expense'}>
        {editing !== null && <ExpenseForm expense={editing === 'new' ? undefined : editing} onDone={() => setEditing(null)} />}
      </Modal>

      <ConfirmDialog
        open={deleting !== null}
        title="Delete Expense"
        message={`Delete "${deleting?.name}"?`}
        onConfirm={handleConfirmDelete}
        onCancel={() => setDeleting(null)}
      />
    </>
  )
}
