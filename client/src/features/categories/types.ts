import type { CategoryType } from '../../lib/types'

/** Mirrors Features/Categories/GetCategories/CategoryVm.cs */
export interface Category {
  id: string
  name: string
  type: CategoryType
}

/** Mirrors Features/Categories/CreateCategory/CreateCategoryCommand.cs */
export interface CreateCategoryRequest {
  name: string
  type: CategoryType
}

/** Mirrors Features/Categories/UpdateCategory/UpdateCategoryCommand.cs */
export interface UpdateCategoryRequest extends CreateCategoryRequest {
  id: string
}
