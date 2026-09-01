# Delete Category

**Endpoint:** `DELETE /api/categories/{id}`

Removes a category.

**Behavior**
- Returns 404 Not Found if the category doesn't exist.
- Open question: what happens to Income/Expense/Budget records that reference this category — block deletion (409 Conflict) if it's in use, or cascade/clear the reference? Needs a decision before implementation.
