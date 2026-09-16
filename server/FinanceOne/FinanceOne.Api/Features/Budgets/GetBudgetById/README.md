# Get Budget By Id

**Endpoint:** `GET /api/budgets/{id}`

Returns a single budget, with its limit and current usage for the month.

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- "Current usage for the month" is the sum of every recurring expense assigned to the budget's
  category, regardless of which day of the month it recurs on.
