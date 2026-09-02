using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Budgets.DeleteBudget;

public interface IDeleteBudgetRepository
{
    Task<Budget?> GetById(Guid id, CancellationToken cancellationToken);
    Task Delete(Budget budget, CancellationToken cancellationToken);
}

public sealed class DeleteBudgetRepository(FinanceOneDbContext context) : IDeleteBudgetRepository
{
    public Task<Budget?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Budgets.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task Delete(Budget budget, CancellationToken cancellationToken)
    {
        context.Budgets.Remove(budget);
        await context.SaveChangesAsync(cancellationToken);
    }
}
