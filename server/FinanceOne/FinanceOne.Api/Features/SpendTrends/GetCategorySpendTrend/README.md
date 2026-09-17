# Get Category Spend Trend

**Endpoint:** `GET /api/spend-trends/{categoryId}`

Returns one expense category's actual monthly spend over the last 6 calendar months, with its
budget limit alongside it if one exists — for the "how is spending in this category trending, and
against what budget" dashboard the point-in-time `GetBudgets` view doesn't answer (issue #49).

**Behavior**
- Returns 404 Not Found if `categoryId` doesn't exist or isn't an `Expense` category.
- **Data source is imported `Transaction` rows only** — never the planned recurring `Expense`
  table. A category with no imported transactions in a given month reports `0` for that month
  rather than falling back to its planned expense amount, so the trend never mixes "planned" and
  "actual" figures. If the category (or the whole account) has no transactions imported at all,
  every month in the response is `0` — the frontend is expected to show an import prompt instead
  of a zeroed-out chart in that case, since this endpoint has no way to distinguish "genuinely
  spent nothing" from "nothing imported yet".
- **Time range is fixed**: the current calendar month and the 5 before it, oldest first — always 6
  entries, including months with no transactions at all (reported as `0`, not omitted). A
  user-selectable range was considered and deferred (see issue #49).
- `monthlyLimit` is the category's `Budget.MonthlyLimit` (one budget per category, reusing the
  existing `Budgets` data per issue #49) — `null` when the category has no budget, in which case
  the frontend renders the trend without a reference line rather than overlaying `0`.
- `Transaction.Amount` is positive for money in / negative for money out; each month's `actual` is
  the **negated** sum of that month's transactions, so spend comes back as a positive figure
  directly comparable to `monthlyLimit`.

**Read-only slice**
- No commands, only this one query — it doesn't own any data itself.

**Cross-slice reads**
- Queries `Categories`, `Budgets`, and `Transactions` directly through its own repository via
  `FinanceOneDbContext`, per the cross-slice read convention in `server/FinanceOne/CLAUDE.md` — the
  same approach `GetBalanceForecast`/`GetUpcomingPayments` use.
