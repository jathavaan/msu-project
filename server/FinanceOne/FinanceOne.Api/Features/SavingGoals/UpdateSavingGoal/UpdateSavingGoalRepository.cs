using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;

public interface IUpdateSavingGoalRepository
{
    Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UpdateSavingGoalRepository(FinanceOneDbContext context) : IUpdateSavingGoalRepository
{
    public Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.SavingGoals.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
