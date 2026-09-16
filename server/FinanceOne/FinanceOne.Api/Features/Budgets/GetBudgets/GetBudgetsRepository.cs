using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Budgets.GetBudgets;

public interface IGetBudgetsRepository
{
    Task<List<BudgetVm>> GetBudgetsWithUsage(CancellationToken cancellationToken);
}

public sealed class GetBudgetsRepository(FinanceOneDbContext context) : IGetBudgetsRepository
{
    // Expenses are recurring templates, not a dated transaction log — every expense in the
    // category is a guaranteed monthly cost, so "used this month" is the total of all of them,
    // regardless of which day of the month they recur on.
    public Task<List<BudgetVm>> GetBudgetsWithUsage(CancellationToken cancellationToken)
    {
        return context.Budgets
            .Select(b => new BudgetVm(
                b.Id,
                b.CategoryId,
                b.Category!.Name,
                b.MonthlyLimit,
                context.Expenses
                    .Where(e => e.CategoryId == b.CategoryId)
                    .Sum(e => (decimal?)e.Amount) ?? 0m))
            .ToListAsync(cancellationToken);
    }
}
