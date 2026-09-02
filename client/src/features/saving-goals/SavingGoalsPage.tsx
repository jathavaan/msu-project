import { useState } from 'react'
import { Plus, Target } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Button } from '../../components/Button'
import { Modal } from '../../components/Modal'
import { QueryState } from '../../components/QueryState'
import { EmptyState } from '../../components/EmptyState'
import { SavingGoalCard } from './SavingGoalCard'
import { SavingGoalForm } from './SavingGoalForm'
import { useDeleteSavingGoalMutation, useGetSavingGoalsQuery } from './api'
import type { SavingGoal } from './types'

export function SavingGoalsPage() {
  const { data: savingGoals, isLoading, error } = useGetSavingGoalsQuery()
  const [deleteSavingGoal] = useDeleteSavingGoalMutation()
  const [editing, setEditing] = useState<SavingGoal | 'new' | null>(null)

  function handleDelete(savingGoal: SavingGoal) {
    if (window.confirm(`Delete "${savingGoal.name}"?`)) {
      void deleteSavingGoal(savingGoal.id)
    }
  }

  return (
    <>
      <PageHeader
        title="Saving Goals"
        description="Track progress toward what you're saving for."
        actions={
          <Button icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Add Goal
          </Button>
        }
      />

      <QueryState
        isLoading={isLoading}
        error={error}
        isEmpty={savingGoals?.length === 0}
        empty={
          <EmptyState
            icon={<Target size={24} />}
            title="No saving goals yet"
            description="Set a target amount and date for something you're saving toward."
          />
        }
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {savingGoals?.map((savingGoal) => (
            <SavingGoalCard key={savingGoal.id} savingGoal={savingGoal} onEdit={setEditing} onDelete={handleDelete} />
          ))}
        </div>
      </QueryState>

      <Modal open={editing !== null} onClose={() => setEditing(null)} title={editing === 'new' ? 'Add Saving Goal' : 'Edit Saving Goal'}>
        {editing !== null && (
          <SavingGoalForm savingGoal={editing === 'new' ? undefined : editing} onDone={() => setEditing(null)} />
        )}
      </Modal>
    </>
  )
}
