# Get Upcoming Payments

**Endpoint:** `GET /api/upcoming-payments?days=7`

Returns income and expenses due within the next N days, for the "next 7 days" and "upcoming bills" Should-have features.

**Behavior**
- `days` query parameter is optional, defaults to 7.
- Reads recurring income (Income slice) and recurring expenses (Expenses slice) and returns the occurrences that fall within the window, sorted by date.
- Returns an empty list if nothing is due in the window.

**Read-only slice**
- No commands, only this one query — it doesn't own any data itself.

**Open questions**
- Same cross-slice read question as Balance Forecast: does this go through `IIncomeRepository`/`IExpenseRepository`, or its own read query? Whatever's decided for Balance Forecast should apply here too, since both slices need the same "walk recurring items forward" logic.
- Should Saving Goal contributions or Discount Code expiries ever show up in this list, or is it strictly income/expenses?
