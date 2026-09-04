# Add Monthly Saving

**Endpoint:** `POST /api/monthly-savings`

Adds a new recurring monthly amount set aside towards a saving goal.

**Behavior**
- Accepts a name, amount, a saving goal id (must reference an existing saving goal), and a
  recurrence day (day of month, 1-28).
- Returns 404 if the referenced saving goal doesn't exist.
- On success, persists the monthly saving and returns its generated id.
