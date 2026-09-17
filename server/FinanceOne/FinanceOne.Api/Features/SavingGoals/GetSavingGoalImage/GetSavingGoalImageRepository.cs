using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalImage;

public interface IGetSavingGoalImageRepository
{
    Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetSavingGoalImageRepository(FinanceOneDbContext context) : IGetSavingGoalImageRepository
{
    public Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.SavingGoals.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}
