import { apiSlice } from '../../app/apiSlice'
import type { BalanceForecastPoint } from './types'

export const balanceForecastApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getBalanceForecast: builder.query<BalanceForecastPoint[], void>({
      query: () => '/balance-forecast',
      providesTags: [{ type: 'BalanceForecast', id: 'LIST' }],
    }),
  }),
})

export const { useGetBalanceForecastQuery } = balanceForecastApi
