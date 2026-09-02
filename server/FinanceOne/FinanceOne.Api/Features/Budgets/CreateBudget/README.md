# Create Budget

**Endpoint:** `POST /api/budgets`

Sets a monthly budget for a category (e.g. food & drinks).

**Behavior**
- Accepts a category id (must reference an `Expense`-type category) and a monthly limit amount.
- Returns 400/404 if the category doesn't exist or isn't an Expense category.
- Returns 409 Conflict if a budget already exists for that category.
- On success, persists the budget and returns its generated id.
