using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Budgets.GetBudgets;

public interface IGetBudgetsRepository
{
    Task<List<BudgetVm>> GetBudgetsWithUsage(CancellationToken cancellationToken);
}

public sealed class GetBudgetsRepository(FinanceOneDbContext context, TimeProvider timeProvider) : IGetBudgetsRepository
{
    // Expenses are recurring templates rather than a dated transaction log, so
    // "used this month" is the total of the category's recurring expenses whose
    // recurrence day has already occurred this month.
    public Task<List<BudgetVm>> GetBudgetsWithUsage(CancellationToken cancellationToken)
    {
        var today = timeProvider.GetUtcNow().Day;

        return context.Budgets
            .Select(b => new BudgetVm(
                b.Id,
                b.CategoryId,
                b.Category!.Name,
                b.MonthlyLimit,
                context.Expenses
                    .Where(e => e.CategoryId == b.CategoryId && e.RecurrenceDay <= today)
                    .Sum(e => (decimal?)e.Amount) ?? 0m))
            .ToListAsync(cancellationToken);
    }
}
