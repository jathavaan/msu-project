# Delete Category

**Endpoint:** `DELETE /api/categories/{id}`

Removes a category.

**Behavior**
- Returns 404 Not Found if the category doesn't exist.
- Returns 409 Conflict if the category is still referenced by any Income, Expense, or Budget record — deletion is blocked rather than cascaded.
