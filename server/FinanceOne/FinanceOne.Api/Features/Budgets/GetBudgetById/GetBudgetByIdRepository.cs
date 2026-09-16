using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Budgets.GetBudgetById;

public interface IGetBudgetByIdRepository
{
    Task<BudgetVm?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetBudgetByIdRepository(FinanceOneDbContext context) : IGetBudgetByIdRepository
{
    // Expenses are recurring templates, not a dated transaction log — every expense in the
    // category is a guaranteed monthly cost, so "used this month" is the total of all of them,
    // regardless of which day of the month they recur on.
    public Task<BudgetVm?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return context.Budgets
            .Where(b => b.Id == id)
            .Select(b => new BudgetVm(
                b.Id,
                b.CategoryId,
                b.Category!.Name,
                b.MonthlyLimit,
                context.Expenses
                    .Where(e => e.CategoryId == b.CategoryId)
                    .Sum(e => (decimal?)e.Amount) ?? 0m))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
