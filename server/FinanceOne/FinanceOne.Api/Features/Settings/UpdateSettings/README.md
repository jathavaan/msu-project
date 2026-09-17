# Update Settings

**Endpoint:** `PUT /api/settings`

Upserts the single global app setting — today, just `periodStartDay`, the day of month the balance
forecast's period starts on (see `Features/BalanceForecast/GetBalanceForecast`'s README).

**Behavior**
- Body: `{ periodStartDay }`. Must be between 1 and 28 inclusive — same cap as
  `Income`/`Expense`/`MonthlySaving.RecurrenceDay`, so the balance forecast can always walk a fixed
  28-day period starting from this day.
- **Upsert, not update-by-id**: if no `AppSettings` row exists yet, one is created; otherwise the
  existing row is updated. There is exactly one row for the whole app (see "Single global setting"
  below), so there's no id in the route or body to look it up by.
- Returns `204 No Content` on success, `400` on validation failure.

**Single global setting, not per-user**
- No multi-user auth exists yet (see `server/FinanceOne/CLAUDE.md`), so this upserts one row for
  the whole app rather than scoping it per user. Revisit if/when auth lands.

**Status:** implemented.
