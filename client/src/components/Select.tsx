import type { SelectHTMLAttributes } from 'react'
import clsx from 'clsx'

interface SelectOption {
  value: string
  label: string
}

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string
  options: SelectOption[]
  error?: string
  placeholder?: string
}

export function Select({ label, options, error, placeholder, className, id, ...rest }: SelectProps) {
  const selectId = id ?? label.toLowerCase().replace(/\s+/g, '-')

  return (
    <label htmlFor={selectId} className="block">
      <span className="mb-1 block text-sm font-medium text-ink-muted">{label}</span>
      <select
        id={selectId}
        className={clsx(
          'w-full rounded-lg border bg-surface px-3 py-2 text-sm text-ink outline-none focus:ring-2 focus:ring-sidebar-active',
          error ? 'border-negative' : 'border-border',
          className,
        )}
        {...rest}
      >
        {placeholder && (
          <option value="" disabled>
            {placeholder}
          </option>
        )}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {error && <span className="mt-1 block text-xs text-negative">{error}</span>}
    </label>
  )
}
