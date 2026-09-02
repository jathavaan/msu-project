import { Pencil, Trash2, Ticket } from 'lucide-react'
import { Button } from '../../components/Button'
import { formatDate, daysUntil } from '../../lib/formatters'
import type { DiscountCode } from './types'

interface DiscountCodeCardProps {
  discountCode: DiscountCode
  onEdit: (discountCode: DiscountCode) => void
  onDelete: (discountCode: DiscountCode) => void
}

export function DiscountCodeCard({ discountCode, onEdit, onDelete }: DiscountCodeCardProps) {
  const days = daysUntil(discountCode.expiryDate)
  const isExpired = days < 0
  const isExpiringSoon = !isExpired && days <= 7

  return (
    <div className="rounded-xl border border-border p-4">
      <div className="mb-3 flex items-start justify-between">
        <div className="flex items-center gap-2">
          {discountCode.codeImageUrl ? (
            <img src={discountCode.codeImageUrl} alt="" className="h-9 w-9 rounded-lg object-cover" />
          ) : (
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-page text-ink-faint">
              <Ticket size={16} />
            </div>
          )}
          <div>
            <p className="text-sm font-semibold text-ink">{discountCode.storeName}</p>
            {discountCode.codeText && <p className="font-mono text-xs text-ink-muted">{discountCode.codeText}</p>}
          </div>
        </div>
        <div className="flex gap-1">
          <Button variant="ghost" size="sm" onClick={() => onEdit(discountCode)}>
            <Pencil size={14} />
          </Button>
          <Button variant="ghost" size="sm" onClick={() => onDelete(discountCode)}>
            <Trash2 size={14} />
          </Button>
        </div>
      </div>
      <span
        className={`inline-block rounded-full px-2.5 py-1 text-xs font-medium ${
          isExpired
            ? 'bg-negative-soft text-negative'
            : isExpiringSoon
              ? 'bg-warning-soft text-warning'
              : 'bg-page text-ink-muted'
        }`}
      >
        {isExpired ? 'Expired' : `Expires ${formatDate(discountCode.expiryDate)}`}
      </span>
    </div>
  )
}
