import { createApi } from '@reduxjs/toolkit/query/react'
import { apiBaseQuery } from '../lib/apiBaseQuery'

/**
 * The single RTK Query instance for the app. It defines no endpoints itself — every feature
 * folder injects its own via `apiSlice.injectEndpoints`, so each feature owns its own "slice" of
 * the API surface (features/<name>/api.ts), mirroring the backend's per-feature endpoint groups.
 */
export const apiSlice = createApi({
  reducerPath: 'api',
  baseQuery: apiBaseQuery,
  tagTypes: ['Category', 'Income', 'Expense', 'Budget', 'SavingGoal', 'DiscountCode', 'UpcomingPayment'],
  endpoints: () => ({}),
})
