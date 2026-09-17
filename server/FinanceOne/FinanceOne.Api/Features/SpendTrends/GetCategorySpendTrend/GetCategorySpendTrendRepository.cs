using FinanceOne.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SpendTrends.GetCategorySpendTrend;

public interface IGetCategorySpendTrendRepository
{
    Task<(string? Name, CategoryType Type, decimal? MonthlyLimit)> GetCategory(Guid categoryId, CancellationToken cancellationToken);

    Task<List<(int Year, int Month, decimal Total)>> GetMonthlySpend(
        Guid categoryId, DateOnly from, CancellationToken cancellationToken);
}

// This slice owns no table of its own — it reads Categories/Budgets for the category's name and
// limit, and Transactions (imported actuals, not the planned Expenses table) for what was spent
// each month, per the cross-slice read convention in server/FinanceOne/CLAUDE.md.
public sealed class GetCategorySpendTrendRepository(FinanceOneDbContext context) : IGetCategorySpendTrendRepository
{
    // Name is null only when no category matches — Category.Name is required, so a real category
    // never produces a null here. The handler uses that to tell "not found" apart from a found
    // category that simply has no budget (MonthlyLimit stays null in that case instead).
    public Task<(string? Name, CategoryType Type, decimal? MonthlyLimit)> GetCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories
            .Where(c => c.Id == categoryId)
            .Select(c => new ValueTuple<string?, CategoryType, decimal?>(
                c.Name, c.Type, c.Budget != null ? c.Budget.MonthlyLimit : (decimal?)null))
            .FirstOrDefaultAsync(cancellationToken);

    // Amount is positive for money in / negative for money out (see Domain/Entites/Transaction.cs),
    // so an expense category's transactions land negative — negate the sum so "spend" comes back
    // as a positive figure directly comparable to Budget.MonthlyLimit.
    public Task<List<(int Year, int Month, decimal Total)>> GetMonthlySpend(
        Guid categoryId, DateOnly from, CancellationToken cancellationToken) =>
        context.Transactions
            .Where(t => t.CategoryId == categoryId && t.Date >= from)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new ValueTuple<int, int, decimal>(g.Key.Year, g.Key.Month, -g.Sum(t => t.Amount)))
            .ToListAsync(cancellationToken);
}
