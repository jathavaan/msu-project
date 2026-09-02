import type { ReactNode } from 'react'
import { Card } from '../../components/Card'

interface StatTileProps {
  label: string
  value: string
  icon: ReactNode
  tone?: 'positive' | 'negative' | 'neutral'
}

const TONE_CLASSES: Record<NonNullable<StatTileProps['tone']>, string> = {
  positive: 'text-positive',
  negative: 'text-negative',
  neutral: 'text-ink',
}

export function StatTile({ label, value, icon, tone = 'neutral' }: StatTileProps) {
  return (
    <Card className="flex items-center gap-3">
      <div className="rounded-lg bg-page p-2 text-ink-muted">{icon}</div>
      <div>
        <p className="text-xs text-ink-muted">{label}</p>
        <p className={`text-lg font-semibold ${TONE_CLASSES[tone]}`}>{value}</p>
      </div>
    </Card>
  )
}
