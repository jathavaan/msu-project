# Update Expense

**Endpoint:** `PUT /api/expenses/{id}`

Edits an existing expense (name, amount, category, recurrence).

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Same category validation as Add Expense.
