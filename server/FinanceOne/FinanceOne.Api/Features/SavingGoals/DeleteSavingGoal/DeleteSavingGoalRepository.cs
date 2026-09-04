using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;

public interface IDeleteSavingGoalRepository
{
    Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken);
    Task<bool> IsReferenced(Guid savingGoalId, CancellationToken cancellationToken);
    Task Delete(SavingGoal savingGoal, CancellationToken cancellationToken);
}

public sealed class DeleteSavingGoalRepository(FinanceOneDbContext context) : IDeleteSavingGoalRepository
{
    public Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.SavingGoals.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<bool> IsReferenced(Guid savingGoalId, CancellationToken cancellationToken) =>
        context.MonthlySavings.AnyAsync(m => m.SavingGoalId == savingGoalId, cancellationToken);

    public async Task Delete(SavingGoal savingGoal, CancellationToken cancellationToken)
    {
        context.SavingGoals.Remove(savingGoal);
        await context.SaveChangesAsync(cancellationToken);
    }
}
