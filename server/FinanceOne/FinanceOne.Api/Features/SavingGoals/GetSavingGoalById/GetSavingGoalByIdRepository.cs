using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;

public interface IGetSavingGoalByIdRepository
{
    Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetSavingGoalByIdRepository(FinanceOneDbContext context) : IGetSavingGoalByIdRepository
{
    public Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.SavingGoals.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}
