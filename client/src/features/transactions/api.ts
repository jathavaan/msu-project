import { apiSlice } from '../../app/apiSlice'
import type { Transaction } from './types'

export const transactionsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getTransactions: builder.query<Transaction[], void>({
      query: () => '/transactions',
      providesTags: (result) => [
        ...(result?.map((t) => ({ type: 'Transaction' as const, id: t.id })) ?? []),
        { type: 'Transaction' as const, id: 'LIST' },
      ],
    }),
  }),
})

export const { useGetTransactionsQuery } = transactionsApi
