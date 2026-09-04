# List Saving Goals

**Endpoint:** `GET /api/saving-goals`

Returns all saving goals with their progress: amount saved so far, amount remaining, and time remaining until the target date.

**Behavior**
- Returns an empty list if none exist.
- "Amount saved so far" is `SavingGoal.CurrentAmount`, a plain field manually tracked on the goal (defaults to 0 on create, editable via Update Saving Goal). There's no dedicated "contribution" command yet — updating progress means calling `PUT /api/saving-goals/{id}` with the new `CurrentAmount`.
- Each goal in the response includes `AmountSaved` (= `CurrentAmount`), `AmountRemaining` (= `TargetAmount - CurrentAmount`), and `DaysRemaining` (days until `TargetDate`, floored at 0 once past due).
- Also includes `MonthlyContribution`: the sum of `Amount` across every Monthly Saving record (see `Features/MonthlySavings`) linked to this goal via `SavingGoalId` — 0 if none exist. This is the recurring monthly rate the goal is being funded at, distinct from `AmountSaved`'s cumulative total.
