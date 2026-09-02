# Get Balance Forecast

**Endpoint:** `GET /api/balance-forecast?month=`

Projects the account balance day-by-day across a given month, for the balance forecast graph on the README's Must-have list.

**Behavior**
- `month` query parameter (e.g. `2026-09`) selects which month to project; defaults to the current month if omitted.
- Reads recurring income (Income slice) and recurring expenses (Expenses slice) whose recurrence falls within the requested month, and walks the days of the month applying each as it occurs to produce a running balance.
- Returns a list of points: `{ date, balance }`, one per day (or per day where the balance changes — cheaper payload, same graph).

**Read-only slice**
- No commands, only this one query — it doesn't own any data itself.

**Open questions before this can actually be implemented**
- **Starting balance:** the projection needs a "balance as of today" to project forward from, but nothing in the slices so far models a current account balance. Does the user enter this manually, or is it computed from history? This needs an owner before Balance Forecast can be built.
- **Cross-slice reads:** this is the first slice that needs data from two other slices (Income, Expenses) rather than owning its own table. Worth deciding now whether it reads through their repository interfaces (e.g. `IIncomeRepository`, `IExpenseRepository`) or queries the shared `AppDbContext` directly — the former respects the SRP split we set up per-slice, the latter is simpler but reaches across slice boundaries. (Resolved for the rest of the codebase: each slice's own repository queries `FinanceOneDbContext` directly, even for tables it doesn't own — see `GetUpcomingPayments` for an example. The same approach would apply here.)

**Status:** deliberately not implemented yet — deferred until the starting-balance question above has an owner. Every other slice in this feature set has been built.
