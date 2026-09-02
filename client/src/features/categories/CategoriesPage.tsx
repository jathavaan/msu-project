import { useState } from 'react'
import { Plus, Pencil, Trash2, Tags } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Card } from '../../components/Card'
import { Button } from '../../components/Button'
import { Modal } from '../../components/Modal'
import { QueryState } from '../../components/QueryState'
import { EmptyState } from '../../components/EmptyState'
import { CategoryBadge } from './CategoryBadge'
import { CategoryForm } from './CategoryForm'
import { useDeleteCategoryMutation, useGetCategoriesQuery } from './api'
import type { Category } from './types'

export function CategoriesPage() {
  const { data: categories, isLoading, error } = useGetCategoriesQuery({})
  const [deleteCategory] = useDeleteCategoryMutation()
  const [editing, setEditing] = useState<Category | 'new' | null>(null)

  return (
    <>
      <PageHeader
        title="Categories"
        description="Group your income and expenses so budgets and reports make sense."
        actions={
          <Button icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Add Category
          </Button>
        }
      />

      <Card>
        <QueryState
          isLoading={isLoading}
          error={error}
          isEmpty={categories?.length === 0}
          empty={
            <EmptyState
              icon={<Tags size={24} />}
              title="No categories yet"
              description="Create your first category to start organizing income and expenses."
            />
          }
        >
          <ul className="divide-y divide-border">
            {categories?.map((category) => (
              <li key={category.id} className="flex items-center justify-between py-3">
                <CategoryBadge category={category} />
                <div className="flex gap-1">
                  <Button variant="ghost" size="sm" onClick={() => setEditing(category)}>
                    <Pencil size={14} />
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => deleteCategory(category.id)}>
                    <Trash2 size={14} />
                  </Button>
                </div>
              </li>
            ))}
          </ul>
        </QueryState>
      </Card>

      <Modal open={editing !== null} onClose={() => setEditing(null)} title={editing === 'new' ? 'Add Category' : 'Edit Category'}>
        {editing !== null && (
          <CategoryForm category={editing === 'new' ? undefined : editing} onDone={() => setEditing(null)} />
        )}
      </Modal>
    </>
  )
}
