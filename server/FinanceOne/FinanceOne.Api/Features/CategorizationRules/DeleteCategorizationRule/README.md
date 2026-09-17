# Delete Categorization Rule

**Endpoint:** `DELETE /api/categorization-rules/{id}`

Removes a keyword rule. Transactions already categorized by it keep their assigned category —
this only stops the rule from matching future imports.

**Behavior**
- Returns 404 Not Found if the rule doesn't exist.
- On success, returns 204 No Content.
