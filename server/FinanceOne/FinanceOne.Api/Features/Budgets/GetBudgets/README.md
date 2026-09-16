# List Budgets

**Endpoint:** `GET /api/budgets`

Returns all budgets, each with its limit and how much of it has been used this month.

**Behavior**
- Returns an empty list if none exist.
- "Used this month" is the sum of every recurring expense assigned to the budget's category,
  regardless of which day of the month it recurs on — expenses are recurring templates (a
  guaranteed monthly cost), not a dated transaction log, so there is no "not spent yet" state to
  exclude. Same cross-slice read question flagged for Balance Forecast/Upcoming Payments.
