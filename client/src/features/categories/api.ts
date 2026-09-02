import { apiSlice } from '../../app/apiSlice'
import type { CategoryType } from '../../lib/types'
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from './types'

export const categoriesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getCategories: builder.query<Category[], { type?: CategoryType }>({
      query: ({ type } = {}) => ({ url: '/categories', params: type ? { type } : undefined }),
      providesTags: (result) => [
        ...(result?.map((c) => ({ type: 'Category' as const, id: c.id })) ?? []),
        { type: 'Category' as const, id: 'LIST' },
      ],
    }),
    createCategory: builder.mutation<string, CreateCategoryRequest>({
      query: (body) => ({ url: '/categories', method: 'POST', body }),
      invalidatesTags: [{ type: 'Category', id: 'LIST' }],
    }),
    updateCategory: builder.mutation<void, UpdateCategoryRequest>({
      query: ({ id, ...body }) => ({ url: `/categories/${id}`, method: 'PUT', body: { id, ...body } }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Category', id },
        { type: 'Category', id: 'LIST' },
      ],
    }),
    deleteCategory: builder.mutation<void, string>({
      query: (id) => ({ url: `/categories/${id}`, method: 'DELETE' }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Category', id },
        { type: 'Category', id: 'LIST' },
      ],
    }),
  }),
})

export const {
  useGetCategoriesQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation,
} = categoriesApi
