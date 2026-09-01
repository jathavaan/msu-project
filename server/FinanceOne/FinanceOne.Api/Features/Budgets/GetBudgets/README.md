# List Budgets

**Endpoint:** `GET /api/budgets`

Returns all budgets, each with its limit and how much of it has been used this month.

**Behavior**
- Returns an empty list if none exist.
- "Used this month" requires reading actual expenses against the budget's category for the current month — same cross-slice read question flagged for Balance Forecast/Upcoming Payments.
