# List Categorization Rules

**Endpoint:** `GET /api/categorization-rules`

Returns every keyword rule used to auto-categorize imported transactions, ordered by keyword.

**Behavior**
- Returns an empty list if none exist.
- Each entry includes the resolved category name alongside its id, for display without a second
  lookup.
