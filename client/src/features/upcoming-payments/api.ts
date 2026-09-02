import { apiSlice } from '../../app/apiSlice'
import type { UpcomingPayment } from './types'

export const upcomingPaymentsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getUpcomingPayments: builder.query<UpcomingPayment[], { days?: number }>({
      query: ({ days } = {}) => ({ url: '/upcoming-payments', params: days ? { days } : undefined }),
      providesTags: [{ type: 'UpcomingPayment', id: 'LIST' }],
    }),
  }),
})

export const { useGetUpcomingPaymentsQuery } = upcomingPaymentsApi
