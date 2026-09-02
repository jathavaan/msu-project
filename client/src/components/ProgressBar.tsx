import clsx from 'clsx'

type Tone = 'positive' | 'warning' | 'negative'

interface ProgressBarProps {
  value: number
  max: number
  tone?: Tone
}

const TONE_CLASSES: Record<Tone, string> = {
  positive: 'bg-positive',
  warning: 'bg-warning',
  negative: 'bg-negative',
}

export function ProgressBar({ value, max, tone }: ProgressBarProps) {
  const ratio = max > 0 ? Math.min(value / max, 1) : 0
  const resolvedTone: Tone = tone ?? (ratio >= 1 ? 'negative' : ratio >= 0.8 ? 'warning' : 'positive')

  return (
    <div className="h-2 w-full overflow-hidden rounded-full bg-page">
      <div
        className={clsx('h-full rounded-full transition-all', TONE_CLASSES[resolvedTone])}
        style={{ width: `${ratio * 100}%` }}
      />
    </div>
  )
}
