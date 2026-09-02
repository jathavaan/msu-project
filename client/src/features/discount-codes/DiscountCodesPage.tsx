import { useState } from 'react'
import { Plus, Ticket } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Button } from '../../components/Button'
import { Modal } from '../../components/Modal'
import { Select } from '../../components/Select'
import { QueryState } from '../../components/QueryState'
import { EmptyState } from '../../components/EmptyState'
import { DiscountCodeCard } from './DiscountCodeCard'
import { DiscountCodeForm } from './DiscountCodeForm'
import { useDeleteDiscountCodeMutation, useGetDiscountCodesQuery } from './api'
import type { DiscountCode } from './types'

const EXPIRY_FILTER_OPTIONS = [
  { value: '', label: 'All codes' },
  { value: '7', label: 'Expiring within 7 days' },
  { value: '30', label: 'Expiring within 30 days' },
]

export function DiscountCodesPage() {
  const [expiryFilter, setExpiryFilter] = useState('')
  const { data: discountCodes, isLoading, error } = useGetDiscountCodesQuery({
    expiringWithinDays: expiryFilter ? Number(expiryFilter) : undefined,
  })
  const [deleteDiscountCode] = useDeleteDiscountCodeMutation()
  const [editing, setEditing] = useState<DiscountCode | 'new' | null>(null)

  function handleDelete(discountCode: DiscountCode) {
    if (window.confirm(`Delete the code for "${discountCode.storeName}"?`)) {
      void deleteDiscountCode(discountCode.id)
    }
  }

  return (
    <>
      <PageHeader
        title="Discount Codes"
        description="Store discount codes so you don't forget to use them before they expire."
        actions={
          <div className="flex items-center gap-3">
            <Select
              label="Filter by expiry"
              value={expiryFilter}
              onChange={(event) => setExpiryFilter(event.target.value)}
              options={EXPIRY_FILTER_OPTIONS}
              className="w-56"
            />
            <Button icon={<Plus size={16} />} onClick={() => setEditing('new')}>
              Add Code
            </Button>
          </div>
        }
      />

      <QueryState
        isLoading={isLoading}
        error={error}
        isEmpty={discountCodes?.length === 0}
        empty={
          <EmptyState
            icon={<Ticket size={24} />}
            title="No discount codes yet"
            description="Save a code before you forget it, and get notified before it expires."
          />
        }
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {discountCodes?.map((discountCode) => (
            <DiscountCodeCard key={discountCode.id} discountCode={discountCode} onEdit={setEditing} onDelete={handleDelete} />
          ))}
        </div>
      </QueryState>

      <Modal open={editing !== null} onClose={() => setEditing(null)} title={editing === 'new' ? 'Add Discount Code' : 'Edit Discount Code'}>
        {editing !== null && (
          <DiscountCodeForm discountCode={editing === 'new' ? undefined : editing} onDone={() => setEditing(null)} />
        )}
      </Modal>
    </>
  )
}
