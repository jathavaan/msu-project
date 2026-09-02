# Create Saving Goal

**Endpoint:** `POST /api/saving-goals`

Creates a new saving goal (e.g. "Emergency fund", target amount, target date).

**Behavior**
- Accepts a name, target amount, and target date.
- Returns 400 if the target date is in the past or the target amount isn't positive.
- On success, persists the goal and returns its generated id.
