import { apiSlice } from '../../app/apiSlice'
import type { CreateIncomeRequest, Income, UpdateIncomeRequest } from './types'

export const incomeApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getIncomes: builder.query<Income[], void>({
      query: () => '/income',
      providesTags: (result) => [
        ...(result?.map((i) => ({ type: 'Income' as const, id: i.id })) ?? []),
        { type: 'Income' as const, id: 'LIST' },
      ],
    }),
    createIncome: builder.mutation<string, CreateIncomeRequest>({
      query: (body) => ({ url: '/income', method: 'POST', body }),
      invalidatesTags: [{ type: 'Income', id: 'LIST' }],
    }),
    updateIncome: builder.mutation<void, UpdateIncomeRequest>({
      query: ({ id, ...body }) => ({ url: `/income/${id}`, method: 'PUT', body: { id, ...body } }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Income', id },
        { type: 'Income', id: 'LIST' },
      ],
    }),
    deleteIncome: builder.mutation<void, string>({
      query: (id) => ({ url: `/income/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Income', id },
        { type: 'Income', id: 'LIST' },
      ],
    }),
  }),
})

export const { useGetIncomesQuery, useCreateIncomeMutation, useUpdateIncomeMutation, useDeleteIncomeMutation } =
  incomeApi
