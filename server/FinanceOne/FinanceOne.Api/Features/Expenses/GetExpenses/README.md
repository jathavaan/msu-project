# List Expenses

**Endpoint:** `GET /api/expenses?categoryId=`

Returns all recurring expenses, optionally filtered by category.

**Behavior**
- `categoryId` query parameter is optional; when omitted, returns all expenses.
- Returns an empty list if none exist.
- Feeds the Balance Forecast and Upcoming Payments slices.
- Covers the README's "subscriptions overview" via filtering by the Subscriptions category rather than a dedicated endpoint — flagging in case a separate endpoint is actually wanted.
