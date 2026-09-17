# Get Balance Forecast

**Endpoint:** `GET /api/balance-forecast`

Projects the running balance day-by-day across the project's 28-day period, for the "Graph that
shows a balance forecast for the month" Must-have feature — the dashboard's income-vs-expenses
balance graph.

**Behavior**
- No parameters — the period is always a fixed 28-day walk. Recurrence (`Income.RecurrenceDay` /
  `Expense.RecurrenceDay` / `MonthlySaving.RecurrenceDay`) is capped at 1-28 specifically so every
  month can be walked as this same fixed period regardless of which month it actually is or how
  many days it has.
- Reads recurring income (Income slice), recurring expenses (Expenses slice), and recurring
  monthly savings (MonthlySavings slice), and walks the period applying each occurrence as it
  falls due to produce a running balance. A monthly saving leaving the account counts against the
  balance the same way an expense does — see "Savings dip the balance" below.
- The walk starts from `GetSettings`'s `periodStartDay` (default 1) instead of always day 1, and
  wraps across the month boundary — a start day of 25 walks 25, 26, 27, 28, 1, 2, ... 24. See
  "Custom period start day" below.
- Returns one point per day: `{ day, balance, incomes, expenses, savings }`, where
  `incomes`/`expenses`/`savings` are the individual entries (`{ name, categoryName, amount }`)
  applied that day — this is what drives the graph's hover tooltip, so a spike or dip can be
  traced back to what actually caused it, not just the resulting number. For a savings entry,
  `categoryName` is the linked `SavingGoal`'s name (a `MonthlySaving` links to a `SavingGoal`, not
  a `Category` — see `Domain/Entites/MonthlySaving.cs`).
- `day` is the actual calendar day-of-month the point falls on (not the point's position in the
  list) — the two only coincide when `periodStartDay` is 1.
- The balance starts from the **rolled-over total net of the period** (see "Starting balance"
  below), not 0, and moves relative to that as income/expenses/savings land.

**Savings dip the balance**
- Each `MonthlySaving` is treated like an expense: its amount is subtracted from the running
  balance (and from the rolled-over starting balance) on its `RecurrenceDay`, the same way an
  `Expense` already is. The account balance really does drop when money is set aside into a saving
  goal, so this keeps the chart to one series rather than drawing savings as a separate line.

**Custom period start day**
- Comes from `Features/Settings/GetSettings` (`AppSettings.PeriodStartDay`), a single app-wide
  value — no multi-user auth exists yet, so there's no per-user setting to read instead (see
  `server/FinanceOne/CLAUDE.md`). No settings row yet defaults to day 1, matching the old
  always-day-1 behavior exactly.
- The wraparound is `(periodStartDay - 1 + offset) % 28 + 1` for `offset` in `0..27` — this is the
  same 1-28 day-of-month space `RecurrenceDay` already lives in, just walked starting from a
  different point.

**Read-only slice**
- No commands, only this one query — it doesn't own any data itself.

**Starting balance (resolved)**
- Nothing in the domain models an actual account balance to project forward from (no slice owns
  a "current balance" concept). The balance here is still **relative**, not an absolute account
  figure — but it no longer resets to 0 on day 1 of every period.
- Every month walks the exact same recurring income/expenses/savings, so last month's ending
  balance is always `totalNet` (all recurring income minus all recurring expenses and savings for
  the 28-day period) more than it started. That rolled-over total is this month's starting balance
  too — the first point starts at `totalNet` and the last ends at `2 × totalNet` — rather than
  every month artificially starting back at 0.
- If a "starting balance" concept (an actual account balance to seed the very first period) is
  introduced elsewhere later, this slice can add it on top of the rolled-over total instead of
  changing this shape.

**Cross-slice reads**
- Queries `Incomes`/`Expenses`/`MonthlySavings`/`AppSettings` directly through its own repository
  via `FinanceOneDbContext`, per the cross-slice read convention in `server/FinanceOne/CLAUDE.md`
  (own repository, no dependency on another slice's repository interface) — the same approach
  `GetUpcomingPayments` uses.

**Status:** implemented.
