import { apiSlice } from '../../app/apiSlice'
import type { CreateExpenseRequest, Expense, UpdateExpenseRequest } from './types'

export const expensesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getExpenses: builder.query<Expense[], { categoryId?: string }>({
      query: ({ categoryId } = {}) => ({ url: '/expenses', params: categoryId ? { categoryId } : undefined }),
      providesTags: (result) => [
        ...(result?.map((e) => ({ type: 'Expense' as const, id: e.id })) ?? []),
        { type: 'Expense' as const, id: 'LIST' },
      ],
    }),
    createExpense: builder.mutation<string, CreateExpenseRequest>({
      query: (body) => ({ url: '/expenses', method: 'POST', body }),
      invalidatesTags: [{ type: 'Expense', id: 'LIST' }],
    }),
    updateExpense: builder.mutation<void, UpdateExpenseRequest>({
      query: ({ id, ...body }) => ({ url: `/expenses/${id}`, method: 'PUT', body: { id, ...body } }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Expense', id },
        { type: 'Expense', id: 'LIST' },
      ],
    }),
    deleteExpense: builder.mutation<void, string>({
      query: (id) => ({ url: `/expenses/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Expense', id },
        { type: 'Expense', id: 'LIST' },
      ],
    }),
  }),
})

export const {
  useGetExpensesQuery,
  useCreateExpenseMutation,
  useUpdateExpenseMutation,
  useDeleteExpenseMutation,
} = expensesApi
