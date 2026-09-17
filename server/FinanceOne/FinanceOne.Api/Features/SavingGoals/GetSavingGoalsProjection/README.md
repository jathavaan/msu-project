# Get Saving Goals Projection

**Endpoint:** `GET /api/saving-goals/projection?years=5`

Projects the **combined** balance of every saving goal forward month by month, for a "total
projected savings over the next N years" chart — see `Features/SavingGoals/GetSavingGoalProjection`'s
README for the per-goal version this builds on.

**Behavior**
- `years` query parameter is optional, defaults to 5 (mirrors `GetUpcomingPayments`'s `days`
  parameter: any value `<= 0` also falls back to the default rather than being rejected).
- Walks **every** saving goal forward using the same per-goal logic as `GetSavingGoalProjection`
  (start from `CurrentAmount`, add `MonthlyContribution` each month, compound `InterestRate`
  monthly), but for the full `years * 12`-month horizon rather than stopping once a goal reaches
  its own `TargetAmount` — a goal that's already met keeps compounding/accumulating in the total
  instead of dropping out of it.
- Returns one point per month, `{ month, date, totalBalance }`, where `totalBalance` is the sum of
  every goal's own running balance that month, each rounded to 2 decimal places before summing.
- With no saving goals at all, every point's `totalBalance` is 0 rather than the endpoint failing
  or returning an empty list — there's nothing to sum, but the horizon itself is still well-defined.

**Read-only slice**
- No commands, only this one query — it doesn't own any data itself, same as `GetSavingGoalProjection`.
- Never fails (no 404/etc. branch) — same shape as `GetBalanceForecast`/`GetUpcomingPayments`,
  which also always return a list rather than a single resource that could be missing.

**Cross-slice reads**
- Queries `SavingGoals` and `MonthlySavings` directly through its own repository via
  `FinanceOneDbContext`, duplicating `GetSavingGoals`'s query shape — expected under this
  codebase's VSA convention (own repository per slice, no dependency on another slice's repository
  interface).

**Status:** implemented.
