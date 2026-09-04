# Update Monthly Saving

**Endpoint:** `PUT /api/monthly-savings/{id}`

Edits an existing monthly saving (name, amount, saving goal, recurrence).

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Same saving goal validation as Add Monthly Saving.
