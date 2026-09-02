import type { ReactNode } from 'react'
import type { SerializedError } from '@reduxjs/toolkit'
import { Spinner } from './Spinner'
import { ErrorBanner } from './ErrorBanner'
import { getErrorMessage } from '../lib/apiBaseQuery'
import type { ApiError } from '../lib/apiBaseQuery'

interface QueryStateProps {
  isLoading: boolean
  error?: ApiError | SerializedError | undefined
  isEmpty?: boolean
  empty?: ReactNode
  children: ReactNode
}

/** Shared loading/error/empty handling so feature pages only need to render the happy path. */
export function QueryState({ isLoading, error, isEmpty, empty, children }: QueryStateProps) {
  if (isLoading) return <Spinner />
  if (error) return <ErrorBanner message={getErrorMessage(error) ?? 'Something went wrong. Please try again.'} />
  if (isEmpty && empty) return <>{empty}</>
  return <>{children}</>
}
