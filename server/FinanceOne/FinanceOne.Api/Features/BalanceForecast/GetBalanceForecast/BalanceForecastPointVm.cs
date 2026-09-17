namespace FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

/// <summary>
/// One income, expense, or monthly saving occurrence applied on a given day, for the graph's
/// hover breakdown. For a saving, CategoryName is the linked SavingGoal's name.
/// </summary>
public sealed record BalanceEntryVm(string Name, string CategoryName, decimal Amount);

/// <summary>
/// Day is the actual calendar day-of-month (1-28) this point falls on, not an offset into the
/// walk — the walk starts at GetBalanceForecastRepository.GetPeriodStartDay and wraps, so Day only
/// equals the point's position in the list when the period starts on day 1.
/// </summary>
public sealed record BalanceForecastPointVm(
    int Day,
    decimal Balance,
    List<BalanceEntryVm> Incomes,
    List<BalanceEntryVm> Expenses,
    List<BalanceEntryVm> Savings);
