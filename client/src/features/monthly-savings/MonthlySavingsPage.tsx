import { useState } from 'react'
import { PiggyBank, Plus } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Card } from '../../components/Card'
import { Button } from '../../components/Button'
import { Modal } from '../../components/Modal'
import { ConfirmDialog } from '../../components/ConfirmDialog'
import { QueryState } from '../../components/QueryState'
import { EmptyState } from '../../components/EmptyState'
import { MonthlySavingTable } from './MonthlySavingTable'
import { MonthlySavingForm } from './MonthlySavingForm'
import { useDeleteMonthlySavingMutation, useGetMonthlySavingsQuery } from './api'
import type { MonthlySaving } from './types'

export function MonthlySavingsPage() {
  const { data: monthlySavings, isLoading, error } = useGetMonthlySavingsQuery()
  const [deleteMonthlySaving] = useDeleteMonthlySavingMutation()
  const [editing, setEditing] = useState<MonthlySaving | 'new' | null>(null)
  const [deleting, setDeleting] = useState<MonthlySaving | null>(null)

  function handleConfirmDelete() {
    if (deleting) void deleteMonthlySaving(deleting.id)
    setDeleting(null)
  }

  return (
    <>
      <PageHeader
        title="Monthly Savings"
        description="Recurring amounts you set aside each month towards a saving goal."
        actions={
          <Button icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Add Monthly Saving
          </Button>
        }
      />

      <Card>
        <QueryState
          isLoading={isLoading}
          error={error}
          isEmpty={monthlySavings?.length === 0}
          empty={
            <EmptyState
              icon={<PiggyBank size={24} />}
              title="No monthly savings yet"
              description="Add a recurring amount to set aside each month towards one of your saving goals."
            />
          }
        >
          <MonthlySavingTable monthlySavings={monthlySavings ?? []} onEdit={setEditing} onDelete={setDeleting} />
        </QueryState>
      </Card>

      <Modal
        open={editing !== null}
        onClose={() => setEditing(null)}
        title={editing === 'new' ? 'Add Monthly Saving' : 'Edit Monthly Saving'}
      >
        {editing !== null && (
          <MonthlySavingForm monthlySaving={editing === 'new' ? undefined : editing} onDone={() => setEditing(null)} />
        )}
      </Modal>

      <ConfirmDialog
        open={deleting !== null}
        title="Delete Monthly Saving"
        message={`Delete "${deleting?.name}"?`}
        onConfirm={handleConfirmDelete}
        onCancel={() => setDeleting(null)}
      />
    </>
  )
}
