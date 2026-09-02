import type { HTMLAttributes, ReactNode } from 'react'
import clsx from 'clsx'

interface CardProps extends Omit<HTMLAttributes<HTMLDivElement>, 'title'> {
  title?: ReactNode
  actions?: ReactNode
}

export function Card({ title, actions, className, children, ...rest }: CardProps) {
  return (
    <div className={clsx('rounded-2xl border border-border bg-surface p-5 shadow-sm', className)} {...rest}>
      {(title ?? actions) && (
        <div className="mb-4 flex items-center justify-between gap-4">
          {title && <h3 className="text-sm font-semibold text-ink">{title}</h3>}
          {actions}
        </div>
      )}
      {children}
    </div>
  )
}
