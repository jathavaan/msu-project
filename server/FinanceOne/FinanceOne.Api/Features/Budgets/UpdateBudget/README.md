# Update Budget

**Endpoint:** `PUT /api/budgets/{id}`

Changes a budget's monthly limit.

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Category is not editable here — delete and recreate the budget to move it to a different category.
