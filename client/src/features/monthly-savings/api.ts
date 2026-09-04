import { apiSlice } from '../../app/apiSlice'
import type { CreateMonthlySavingRequest, MonthlySaving, UpdateMonthlySavingRequest } from './types'

export const monthlySavingsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getMonthlySavings: builder.query<MonthlySaving[], void>({
      query: () => '/monthly-savings',
      providesTags: (result) => [
        ...(result?.map((m) => ({ type: 'MonthlySaving' as const, id: m.id })) ?? []),
        { type: 'MonthlySaving' as const, id: 'LIST' },
      ],
    }),
    createMonthlySaving: builder.mutation<string, CreateMonthlySavingRequest>({
      query: (body) => ({ url: '/monthly-savings', method: 'POST', body }),
      // A new monthly saving changes the goal's MonthlyContribution total too.
      invalidatesTags: [{ type: 'MonthlySaving', id: 'LIST' }, { type: 'SavingGoal', id: 'LIST' }],
    }),
    updateMonthlySaving: builder.mutation<void, UpdateMonthlySavingRequest>({
      query: ({ id, ...body }) => ({ url: `/monthly-savings/${id}`, method: 'PUT', body: { id, ...body } }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'MonthlySaving', id },
        { type: 'MonthlySaving', id: 'LIST' },
        { type: 'SavingGoal', id: 'LIST' },
      ],
    }),
    deleteMonthlySaving: builder.mutation<void, string>({
      query: (id) => ({ url: `/monthly-savings/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'MonthlySaving', id },
        { type: 'MonthlySaving', id: 'LIST' },
        { type: 'SavingGoal', id: 'LIST' },
      ],
    }),
  }),
})

export const {
  useGetMonthlySavingsQuery,
  useCreateMonthlySavingMutation,
  useUpdateMonthlySavingMutation,
  useDeleteMonthlySavingMutation,
} = monthlySavingsApi
