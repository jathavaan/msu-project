namespace FinanceOne.Api.Features.SpendTrends.GetCategorySpendTrend;

/// <summary>One calendar month's actual spend for the category, oldest first.</summary>
public sealed record MonthlySpendVm(int Year, int Month, decimal Actual);

public sealed record CategorySpendTrendVm(
    Guid CategoryId,
    string CategoryName,
    decimal? MonthlyLimit,
    List<MonthlySpendVm> Months);
