using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoals;

public interface IGetSavingGoalsRepository
{
    Task<List<SavingGoal>> GetSavingGoals(CancellationToken cancellationToken);
}

public sealed class GetSavingGoalsRepository(FinanceOneDbContext context) : IGetSavingGoalsRepository
{
    public Task<List<SavingGoal>> GetSavingGoals(CancellationToken cancellationToken) =>
        context.SavingGoals.AsNoTracking().OrderBy(s => s.TargetDate).ToListAsync(cancellationToken);
}
