import { Select } from '../../components/Select'
import { useGetCategoriesQuery } from './api'
import type { CategoryType } from '../../lib/types'

interface CategoryPickerProps {
  type: CategoryType
  value: string
  onChange: (categoryId: string) => void
  error?: string
}

/** Category dropdown scoped to one CategoryType, shared by the Income and Expenses forms. */
export function CategoryPicker({ type, value, onChange, error }: CategoryPickerProps) {
  const { data: categories, isLoading } = useGetCategoriesQuery({ type })

  return (
    <Select
      label="Category"
      value={value}
      onChange={(event) => onChange(event.target.value)}
      options={(categories ?? []).map((category) => ({ value: category.id, label: category.name }))}
      placeholder={isLoading ? 'Loading categories…' : 'Select a category'}
      error={error}
      disabled={isLoading}
      required
    />
  )
}
