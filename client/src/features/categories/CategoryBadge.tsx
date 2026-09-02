import { categoryClasses } from '../../lib/categoryColor'
import type { Category } from './types'

export function CategoryBadge({ category }: { category: Category }) {
  const classes = categoryClasses(category.id)

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full bg-page px-2.5 py-1 text-xs font-medium ${classes.text}`}>
      <span className={`h-1.5 w-1.5 rounded-full ${classes.dot}`} />
      {category.name}
    </span>
  )
}
