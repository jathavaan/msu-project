# Add Expense

**Endpoint:** `POST /api/expenses`

Adds a new recurring expense (e.g. rent, subscription, car, food, electricity).

**Behavior**
- Accepts a name, amount, a category id (must reference an `Expense`-type category), and recurrence details (e.g. day of month due).
- Returns 400/404 if the referenced category doesn't exist or isn't an Expense category.
- On success, persists the expense and returns its generated id.
