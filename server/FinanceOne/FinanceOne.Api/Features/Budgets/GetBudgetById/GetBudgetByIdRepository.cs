using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Budgets.GetBudgetById;

public interface IGetBudgetByIdRepository
{
    Task<BudgetVm?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetBudgetByIdRepository(FinanceOneDbContext context, TimeProvider timeProvider) : IGetBudgetByIdRepository
{
    // Expenses are recurring templates rather than a dated transaction log, so
    // "used this month" is the total of the category's recurring expenses whose
    // recurrence day has already occurred this month.
    public Task<BudgetVm?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var today = timeProvider.GetUtcNow().Day;

        return context.Budgets
            .Where(b => b.Id == id)
            .Select(b => new BudgetVm(
                b.Id,
                b.CategoryId,
                b.Category!.Name,
                b.MonthlyLimit,
                context.Expenses
                    .Where(e => e.CategoryId == b.CategoryId && e.RecurrenceDay <= today)
                    .Sum(e => (decimal?)e.Amount) ?? 0m))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
