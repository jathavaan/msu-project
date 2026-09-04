# Delete Saving Goal

**Endpoint:** `DELETE /api/saving-goals/{id}`

Removes a saving goal.

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Returns 409 Conflict if the goal is still referenced by any Monthly Saving record — deletion is blocked rather than cascaded.
