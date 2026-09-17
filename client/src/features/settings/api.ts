import { apiSlice } from '../../app/apiSlice'
import type { Settings, UpdateSettingsRequest } from './types'

export const settingsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getSettings: builder.query<Settings, void>({
      query: () => '/settings',
      providesTags: ['Settings'],
    }),
    updateSettings: builder.mutation<void, UpdateSettingsRequest>({
      query: (body) => ({ url: '/settings', method: 'PUT', body }),
      // The balance forecast's day-walk depends on periodStartDay, so changing it must refresh
      // the dashboard chart's cached data too, not just the settings themselves.
      invalidatesTags: ['Settings', { type: 'BalanceForecast', id: 'LIST' }],
    }),
  }),
})

export const { useGetSettingsQuery, useUpdateSettingsMutation } = settingsApi
