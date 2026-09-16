# Create Saving Goal

**Endpoint:** `POST /api/saving-goals`

Creates a new saving goal (e.g. "Emergency fund", target amount, target date).

**Behavior**
- Accepts a name, target amount, target date, and an optional `InterestRate` (annual percentage,
  e.g. `4.5` for 4.5%, compounded monthly by `GetSavingGoalProjection`).
- Returns 400 if the target date is in the past, the target amount isn't positive, or
  `InterestRate` is set and outside `0`-`100`.
- `InterestRate` defaults to `null` (no interest) when omitted — the goal behaves exactly as
  before, with a contribution-only projection.
- On success, persists the goal and returns its generated id.
