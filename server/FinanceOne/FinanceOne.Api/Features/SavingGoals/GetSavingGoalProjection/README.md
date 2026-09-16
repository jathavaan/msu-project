# Get Saving Goal Projection

**Endpoint:** `GET /api/saving-goals/{id}/projection`

Projects a saving goal's balance forward month by month, for the "graph of expected balance over
time" and "when will I actually hit my target?" parts of the saving goal feature — see
`Features/SavingGoals/GetSavingGoals`'s README for the fields this builds on.

**Behavior**
- Returns 404 Not Found if the goal doesn't exist.
- Starts from `CurrentAmount` at month 0 (today), then steps forward month by month: each month
  adds the goal's `MonthlyContribution` (the same sum of linked `MonthlySaving.Amount` that
  `GetSavingGoals`/`GetSavingGoalById` already compute) and compounds `InterestRate` monthly
  (`InterestRate / 100 / 12` applied to the running balance) — same shape as the `BalanceForecast`
  slice's time-series projection, but monthly instead of daily and keyed off a saving goal instead
  of recurring income/expenses.
- `InterestRate` is optional; `null` (or `0`) simply contributes no interest, so a goal with no
  rate set projects exactly as it always has — contribution-only.
- Returns `Points`: one `{ month, date, balance }` entry per month, `Balance` rounded to 2 decimal
  places. This powers the projected-balance chart, shown alongside `TargetAmount` as a reference
  line.
- Returns `ReachDate`: the date of the first month the running balance meets or exceeds
  `TargetAmount`, or `null` if it's never projected to get there. Two cases produce `null`:
  - Nothing moves the balance at all (no `MonthlyContribution` and no `InterestRate`) — reported
    immediately without generating a flat 50-year series.
  - The balance is still short after `MaxProjectionMonths` (600 — 50 years), which is treated as
    "not reachable at the current rate" rather than projected further.
- If the goal is already at or past `TargetAmount` (from `CurrentAmount` alone), `Points` is just
  the single month-0 entry and `ReachDate` is today — there's nothing further to project.
- `Points` stops at the reach month once found, rather than continuing to `MaxProjectionMonths` —
  the chart has no use for balance past the point the goal is met.

**Read-only slice**
- No commands, only this one query — it doesn't own any data itself, same as `BalanceForecast`.

**Cross-slice reads**
- Queries `SavingGoals` and `MonthlySavings` directly through its own repository via
  `FinanceOneDbContext`, duplicating the same query shape `GetSavingGoalById`'s repository already
  has — expected under this codebase's VSA convention (own repository per slice, no dependency on
  another slice's repository interface).

**Status:** implemented.
