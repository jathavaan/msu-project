# Get Saving Goal By Id

**Endpoint:** `GET /api/saving-goals/{id}`

Returns a single saving goal with its progress.

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Includes `MonthlyContribution`, the sum of `Amount` across every Monthly Saving record linked to this goal (0 if none) — see `Features/SavingGoals/GetSavingGoals`'s README for details.
