# Update Income

**Endpoint:** `PUT /api/income/{id}`

Edits an existing income source (name, amount, category, recurrence).

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Same category validation as Add Income.
