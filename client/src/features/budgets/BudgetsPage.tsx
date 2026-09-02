import { useState } from 'react'
import { Plus, Wallet } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Button } from '../../components/Button'
import { Modal } from '../../components/Modal'
import { ConfirmDialog } from '../../components/ConfirmDialog'
import { QueryState } from '../../components/QueryState'
import { EmptyState } from '../../components/EmptyState'
import { BudgetCard } from './BudgetCard'
import { BudgetForm } from './BudgetForm'
import { useDeleteBudgetMutation, useGetBudgetsQuery } from './api'
import type { Budget } from './types'

export function BudgetsPage() {
  const { data: budgets, isLoading, error } = useGetBudgetsQuery()
  const [deleteBudget] = useDeleteBudgetMutation()
  const [editing, setEditing] = useState<Budget | 'new' | null>(null)
  const [deleting, setDeleting] = useState<Budget | null>(null)

  function handleConfirmDelete() {
    if (deleting) void deleteBudget(deleting.id)
    setDeleting(null)
  }

  return (
    <>
      <PageHeader
        title="Budgets"
        description="Set a monthly spending limit per category and track how much you've used."
        actions={
          <Button icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Add Budget
          </Button>
        }
      />

      <QueryState
        isLoading={isLoading}
        error={error}
        isEmpty={budgets?.length === 0}
        empty={
          <EmptyState
            icon={<Wallet size={24} />}
            title="No budgets yet"
            description="Set a monthly limit for an expense category to start tracking it."
          />
        }
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {budgets?.map((budget) => (
            <BudgetCard key={budget.id} budget={budget} onEdit={setEditing} onDelete={setDeleting} />
          ))}
        </div>
      </QueryState>

      <Modal open={editing !== null} onClose={() => setEditing(null)} title={editing === 'new' ? 'Add Budget' : 'Edit Budget'}>
        {editing !== null && <BudgetForm budget={editing === 'new' ? undefined : editing} onDone={() => setEditing(null)} />}
      </Modal>

      <ConfirmDialog
        open={deleting !== null}
        title="Delete Budget"
        message={`Delete the budget for "${deleting?.categoryName}"?`}
        onConfirm={handleConfirmDelete}
        onCancel={() => setDeleting(null)}
      />
    </>
  )
}
