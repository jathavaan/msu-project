namespace FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

/// <summary>One income or expense occurrence applied on a given day, for the graph's hover breakdown.</summary>
public sealed record BalanceEntryVm(string Name, string CategoryName, decimal Amount);

public sealed record BalanceForecastPointVm(
    int Day,
    decimal Balance,
    List<BalanceEntryVm> Incomes,
    List<BalanceEntryVm> Expenses);
