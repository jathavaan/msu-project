import { apiSlice } from '../../app/apiSlice'
import type { Budget, CreateBudgetRequest, UpdateBudgetRequest } from './types'

export const budgetsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getBudgets: builder.query<Budget[], void>({
      query: () => '/budgets',
      providesTags: (result) => [
        ...(result?.map((b) => ({ type: 'Budget' as const, id: b.id })) ?? []),
        { type: 'Budget' as const, id: 'LIST' },
      ],
    }),
    createBudget: builder.mutation<string, CreateBudgetRequest>({
      query: (body) => ({ url: '/budgets', method: 'POST', body }),
      invalidatesTags: [{ type: 'Budget', id: 'LIST' }],
    }),
    updateBudget: builder.mutation<void, UpdateBudgetRequest>({
      query: ({ id, ...body }) => ({ url: `/budgets/${id}`, method: 'PUT', body: { id, ...body } }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Budget', id },
        { type: 'Budget', id: 'LIST' },
      ],
    }),
    deleteBudget: builder.mutation<void, string>({
      query: (id) => ({ url: `/budgets/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Budget', id },
        { type: 'Budget', id: 'LIST' },
      ],
    }),
  }),
})

export const { useGetBudgetsQuery, useCreateBudgetMutation, useUpdateBudgetMutation, useDeleteBudgetMutation } =
  budgetsApi
