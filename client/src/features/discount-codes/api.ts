import { apiSlice } from '../../app/apiSlice'
import type { CreateDiscountCodeRequest, DiscountCode, UpdateDiscountCodeRequest } from './types'

export const discountCodesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getDiscountCodes: builder.query<DiscountCode[], { expiringWithinDays?: number }>({
      query: ({ expiringWithinDays } = {}) => ({
        url: '/discount-codes',
        params: expiringWithinDays ? { expiringWithinDays } : undefined,
      }),
      providesTags: (result) => [
        ...(result?.map((d) => ({ type: 'DiscountCode' as const, id: d.id })) ?? []),
        { type: 'DiscountCode' as const, id: 'LIST' },
      ],
    }),
    createDiscountCode: builder.mutation<string, CreateDiscountCodeRequest>({
      query: (body) => ({ url: '/discount-codes', method: 'POST', body }),
      invalidatesTags: [{ type: 'DiscountCode', id: 'LIST' }],
    }),
    updateDiscountCode: builder.mutation<void, UpdateDiscountCodeRequest>({
      query: ({ id, ...body }) => ({ url: `/discount-codes/${id}`, method: 'PUT', body: { id, ...body } }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'DiscountCode', id },
        { type: 'DiscountCode', id: 'LIST' },
      ],
    }),
    deleteDiscountCode: builder.mutation<void, string>({
      query: (id) => ({ url: `/discount-codes/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'DiscountCode', id },
        { type: 'DiscountCode', id: 'LIST' },
      ],
    }),
  }),
})

export const {
  useGetDiscountCodesQuery,
  useCreateDiscountCodeMutation,
  useUpdateDiscountCodeMutation,
  useDeleteDiscountCodeMutation,
} = discountCodesApi
