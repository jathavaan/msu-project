import type { InputHTMLAttributes } from 'react'
import clsx from 'clsx'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string
}

export function Input({ label, error, className, id, ...rest }: InputProps) {
  const inputId = id ?? label.toLowerCase().replace(/\s+/g, '-')

  return (
    <label htmlFor={inputId} className="block">
      <span className="mb-1 block text-sm font-medium text-ink-muted">{label}</span>
      <input
        id={inputId}
        className={clsx(
          'w-full rounded-lg border bg-surface px-3 py-2 text-sm text-ink outline-none focus:ring-2 focus:ring-sidebar-active',
          error ? 'border-negative' : 'border-border',
          className,
        )}
        {...rest}
      />
      {error && <span className="mt-1 block text-xs text-negative">{error}</span>}
    </label>
  )
}
