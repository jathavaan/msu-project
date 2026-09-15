import type { ReactNode } from 'react'
import { Loader2 } from 'lucide-react'
import { Card } from '../../components/Card'

interface StatTileProps {
  label: string
  value: string
  icon: ReactNode
  tone?: 'positive' | 'negative' | 'neutral'
  isLoading?: boolean
}

const TONE_CLASSES: Record<NonNullable<StatTileProps['tone']>, string> = {
  positive: 'text-positive',
  negative: 'text-negative',
  neutral: 'text-ink',
}

export function StatTile({ label, value, icon, tone = 'neutral', isLoading }: StatTileProps) {
  return (
    <Card className="flex items-center gap-3">
      <div className="rounded-lg bg-page p-2 text-ink-muted">{icon}</div>
      <div>
        <p className="text-xs text-ink-muted">{label}</p>
        {isLoading ? (
          <Loader2 size={18} className="mt-1 animate-spin text-ink-faint" />
        ) : (
          <p className={`text-lg font-semibold ${TONE_CLASSES[tone]}`}>{value}</p>
        )}
      </div>
    </Card>
  )
}
