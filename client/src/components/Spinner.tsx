import { Loader2 } from 'lucide-react'

export function Spinner({ size = 20 }: { size?: number }) {
  return (
    <div className="flex items-center justify-center py-8 text-ink-faint">
      <Loader2 size={size} className="animate-spin" />
    </div>
  )
}
