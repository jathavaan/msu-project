# Get Upcoming Payments

**Endpoint:** `GET /api/upcoming-payments?days=7`

Returns income and expenses due within the next N days, for the "next 7 days" and "upcoming bills" Should-have features.

**Behavior**
- `days` query parameter is optional, defaults to 7.
- Reads recurring income (Income slice) and recurring expenses (Expenses slice) and returns the occurrences that fall within the window, sorted by date.
- Returns an empty list if nothing is due in the window.

**Read-only slice**
- No commands, only this one query — it doesn't own any data itself.
- Queries `Incomes`/`Expenses` directly through its own repository via `FinanceOneDbContext`, per the cross-slice read convention in `server/FinanceOne/CLAUDE.md` (own repository, no dependency on another slice's repository interface).
- Strictly income/expenses — Saving Goal contributions and Discount Code expiries do not appear in this list.
