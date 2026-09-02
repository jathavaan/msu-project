using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public interface ICreateBudgetRepository
{
    Task<Category?> GetExpenseCategory(Guid categoryId, CancellationToken cancellationToken);
    Task<bool> BudgetExistsForCategory(Guid categoryId, CancellationToken cancellationToken);
    Task<Guid> Add(Budget budget, CancellationToken cancellationToken);
}

public sealed class CreateBudgetRepository(FinanceOneDbContext context) : ICreateBudgetRepository
{
    public Task<Category?> GetExpenseCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public Task<bool> BudgetExistsForCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Budgets.AnyAsync(b => b.CategoryId == categoryId, cancellationToken);

    public async Task<Guid> Add(Budget budget, CancellationToken cancellationToken)
    {
        context.Budgets.Add(budget);
        await context.SaveChangesAsync(cancellationToken);
        return budget.Id;
    }
}
