# Get Settings

**Endpoint:** `GET /api/settings`

Returns the single global app setting used across the app — today, just the balance forecast's
period start day (see `Features/BalanceForecast/GetBalanceForecast`'s README).

**Behavior**
- No parameters.
- Returns `{ periodStartDay }`. `periodStartDay` defaults to `1` (a plain calendar month) when no
  settings row exists yet — nothing has to be configured before the app is usable.
- Never fails (no 404/etc. branch) — same shape as `GetBalanceForecast`/`GetUpcomingPayments`,
  which also always return a well-defined result rather than a single resource that could be
  missing.

**Read-only slice**
- No commands here — see `Features/Settings/UpdateSettings` for writes.

**Single global setting, not per-user**
- No multi-user auth exists yet (see `server/FinanceOne/CLAUDE.md`), so this is one row for the
  whole app rather than scoped per user. Revisit if/when auth lands.

**Status:** implemented.
