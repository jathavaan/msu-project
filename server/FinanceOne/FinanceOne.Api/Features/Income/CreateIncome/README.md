# Add Income

**Endpoint:** `POST /api/income`

Adds a new recurring income source (e.g. salary).

**Behavior**
- Accepts a name, amount, a category id (must reference an `Income`-type category), and recurrence details (e.g. day of month received).
- Returns 400/404 if the referenced category doesn't exist or isn't an Income category.
- On success, persists the income source and returns its generated id.
