# Update Saving Goal

**Endpoint:** `PUT /api/saving-goals/{id}`

Edits a saving goal's name, target amount, or target date.

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Same validation as Create Saving Goal.
- Also accepts `CurrentAmount` (amount saved so far) — this is the only way to update a goal's progress, since there's no separate "contribution" endpoint.
