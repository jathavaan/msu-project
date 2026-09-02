import { apiSlice } from '../../app/apiSlice'
import type { CreateSavingGoalRequest, SavingGoal, UpdateSavingGoalRequest } from './types'

export const savingGoalsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getSavingGoals: builder.query<SavingGoal[], void>({
      query: () => '/saving-goals',
      providesTags: (result) => [
        ...(result?.map((g) => ({ type: 'SavingGoal' as const, id: g.id })) ?? []),
        { type: 'SavingGoal' as const, id: 'LIST' },
      ],
    }),
    createSavingGoal: builder.mutation<string, CreateSavingGoalRequest>({
      query: (body) => ({ url: '/saving-goals', method: 'POST', body }),
      invalidatesTags: [{ type: 'SavingGoal', id: 'LIST' }],
    }),
    updateSavingGoal: builder.mutation<void, UpdateSavingGoalRequest>({
      query: ({ id, ...body }) => ({ url: `/saving-goals/${id}`, method: 'PUT', body: { id, ...body } }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'SavingGoal', id },
        { type: 'SavingGoal', id: 'LIST' },
      ],
    }),
    deleteSavingGoal: builder.mutation<void, string>({
      query: (id) => ({ url: `/saving-goals/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'SavingGoal', id },
        { type: 'SavingGoal', id: 'LIST' },
      ],
    }),
  }),
})

export const {
  useGetSavingGoalsQuery,
  useCreateSavingGoalMutation,
  useUpdateSavingGoalMutation,
  useDeleteSavingGoalMutation,
} = savingGoalsApi
