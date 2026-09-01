# Update Category

**Endpoint:** `PUT /api/categories/{id}`

Renames a category or changes its type.

**Behavior**
- Returns 404 Not Found if the category doesn't exist.
- Returns 409 Conflict if the new name/type combination collides with another existing category.
