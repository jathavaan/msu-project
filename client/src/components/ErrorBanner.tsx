import { AlertTriangle } from 'lucide-react'

interface ErrorBannerProps {
  message: string
}

export function ErrorBanner({ message }: ErrorBannerProps) {
  return (
    <div className="flex items-center gap-2 rounded-lg bg-negative-soft px-4 py-3 text-sm text-negative">
      <AlertTriangle size={16} />
      <span>{message}</span>
    </div>
  )
}
