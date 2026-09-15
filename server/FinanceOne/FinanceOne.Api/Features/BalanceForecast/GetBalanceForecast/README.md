# Get Balance Forecast

**Endpoint:** `GET /api/balance-forecast`

Projects the running balance day-by-day across the project's 28-day period, for the "Graph that
shows a balance forecast for the month" Must-have feature — the dashboard's income-vs-expenses
balance graph.

**Behavior**
- No parameters — the period is always days 1-28. Recurrence (`Income.RecurrenceDay` /
  `Expense.RecurrenceDay`) is capped at 1-28 specifically so every month can be walked as this
  same fixed period regardless of which month it actually is or how many days it has.
- Reads recurring income (Income slice) and recurring expenses (Expenses slice) and walks days 1
  through 28, applying each occurrence as it falls due to produce a running balance.
- Returns one point per day: `{ day, balance, incomes, expenses }`, where `incomes`/`expenses`
  are the individual entries (`{ name, categoryName, amount }`) applied that day — this is what
  drives the graph's hover tooltip, so a spike or dip can be traced back to what actually caused
  it, not just the resulting number.
- The balance starts from the **rolled-over total net of the period** (see "Starting balance"
  below), not 0, and moves relative to that as income/expenses land.

**Read-only slice**
- No commands, only this one query — it doesn't own any data itself.

**Starting balance (resolved)**
- Nothing in the domain models an actual account balance to project forward from (no slice owns
  a "current balance" concept). The balance here is still **relative**, not an absolute account
  figure — but it no longer resets to 0 on day 1 of every period.
- Every month walks the exact same recurring income/expenses, so last month's ending balance is
  always `totalNet` (all recurring income minus all recurring expenses for the 28-day period) more
  than it started. That rolled-over total is this month's starting balance too — day 1 starts at
  `totalNet` and day 28 ends at `2 × totalNet` — rather than every month artificially starting back
  at 0.
- If a "starting balance" concept (an actual account balance to seed the very first period) is
  introduced elsewhere later, this slice can add it on top of the rolled-over total instead of
  changing this shape.

**Cross-slice reads**
- Queries `Incomes`/`Expenses` directly through its own repository via `FinanceOneDbContext`, per
  the cross-slice read convention in `server/FinanceOne/CLAUDE.md` (own repository, no dependency
  on another slice's repository interface) — the same approach `GetUpcomingPayments` uses.

**Status:** implemented.
