using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.UploadSavingGoalImage;

public interface IUploadSavingGoalImageRepository
{
    Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UploadSavingGoalImageRepository(FinanceOneDbContext context) : IUploadSavingGoalImageRepository
{
    public Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.SavingGoals.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
