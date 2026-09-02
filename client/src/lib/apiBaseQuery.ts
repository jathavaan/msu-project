import { fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import type { BaseQueryFn } from '@reduxjs/toolkit/query/react'
import type { FetchArgs, FetchBaseQueryError } from '@reduxjs/toolkit/query/react'
import type { ApiEnvelope } from './types'

export interface ApiError {
  status?: number
  message: string
}

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

function isEnvelope(data: unknown): data is ApiEnvelope<unknown> {
  return typeof data === 'object' && data !== null && 'errorCode' in data
}

function toApiError(error: FetchBaseQueryError): ApiError {
  const problem = error.data as ProblemDetails | undefined
  if (problem?.errors) {
    return { status: Number(error.status), message: Object.values(problem.errors).flat().join(' ') }
  }
  return {
    status: typeof error.status === 'number' ? error.status : undefined,
    message: problem?.detail ?? problem?.title ?? 'Something went wrong. Please try again.',
  }
}

const rawBaseQuery = fetchBaseQuery({
  baseUrl: `${import.meta.env.VITE_API_BASE_URL}/api`,
})

/**
 * Wraps fetchBaseQuery to unwrap the backend's Response<T> envelope (used by every GET) into a
 * plain result, and to normalize both ProblemDetails and ValidationProblemDetails error bodies
 * into one ApiError shape. Mutating endpoints (POST/PUT/DELETE) return either a plain value or
 * no content at all, so those pass through unchanged.
 */
export const apiBaseQuery: BaseQueryFn<string | FetchArgs, unknown, ApiError> = async (
  args,
  api,
  extraOptions,
) => {
  const response = await rawBaseQuery(args, api, extraOptions)

  if (response.error) {
    return { error: toApiError(response.error) }
  }

  if (isEnvelope(response.data)) {
    const envelope = response.data
    if (envelope.errorCode != null) {
      return { error: { status: envelope.errorCode, message: envelope.errorMessage ?? 'Something went wrong.' } }
    }
    return { data: envelope.result }
  }

  return { data: response.data }
}
