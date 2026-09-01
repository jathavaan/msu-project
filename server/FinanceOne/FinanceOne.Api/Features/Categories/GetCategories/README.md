# List Categories

**Endpoint:** `GET /api/categories?type=`

Returns all categories, optionally filtered by type.

**Behavior**
- `type` query parameter is optional (`Income` or `Expense`); when omitted, returns both.
- Returns an empty list if none exist — not an error.
