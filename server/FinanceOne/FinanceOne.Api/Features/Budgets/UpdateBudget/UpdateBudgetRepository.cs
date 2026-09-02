using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Budgets.UpdateBudget;

public interface IUpdateBudgetRepository
{
    Task<Budget?> GetById(Guid id, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UpdateBudgetRepository(FinanceOneDbContext context) : IUpdateBudgetRepository
{
    public Task<Budget?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Budgets.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
