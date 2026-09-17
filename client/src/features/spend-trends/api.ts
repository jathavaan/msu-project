import { apiSlice } from '../../app/apiSlice'
import type { CategorySpendTrend } from './types'

export const spendTrendsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getCategorySpendTrend: builder.query<CategorySpendTrend, string>({
      query: (categoryId) => `/spend-trends/${categoryId}`,
      providesTags: (_result, _error, categoryId) => [{ type: 'SpendTrend', id: categoryId }],
    }),
  }),
})

export const { useGetCategorySpendTrendQuery } = spendTrendsApi
