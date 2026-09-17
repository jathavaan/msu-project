namespace FinanceOne.Api.Domain.Entites;

// Single global app-wide setting — no multi-user auth exists yet (see server/FinanceOne/CLAUDE.md),
// so one row covers the whole app instead of being scoped per user. Exactly one row is ever
// expected to exist; see Features/Settings/UpdateSettings, which upserts it.
public sealed class AppSettings
{
    public Guid Id { get; init; }

    // Day of month (1-28, same cap as Income/Expense/MonthlySaving.RecurrenceDay) the user's
    // personal "period" starts on — e.g. a payday of the 25th instead of the 1st. No row yet
    // means nothing has been configured, which Features/Settings/GetSettings defaults to 1
    // (a plain calendar month) rather than treating as an error.
    public required int PeriodStartDay { get; set; }
}
