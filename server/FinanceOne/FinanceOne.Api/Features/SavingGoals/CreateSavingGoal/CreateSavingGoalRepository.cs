namespace FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;

public interface ICreateSavingGoalRepository
{
    Task<Guid> Add(SavingGoal savingGoal, CancellationToken cancellationToken);
}

public sealed class CreateSavingGoalRepository(FinanceOneDbContext context) : ICreateSavingGoalRepository
{
    public async Task<Guid> Add(SavingGoal savingGoal, CancellationToken cancellationToken)
    {
        context.SavingGoals.Add(savingGoal);
        await context.SaveChangesAsync(cancellationToken);
        return savingGoal.Id;
    }
}
