# Create Category

**Endpoint:** `POST /api/categories`

Creates a new category used to tag income or expenses.

**Behavior**
- Accepts a name and a type (`Income` or `Expense`).
- Rejects the request if a category with the same name and type already exists (409 Conflict).
- On success, persists the category and returns its generated id.
