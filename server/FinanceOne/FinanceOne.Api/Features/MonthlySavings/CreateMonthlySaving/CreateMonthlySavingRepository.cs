using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;

public interface ICreateMonthlySavingRepository
{
    Task<SavingGoal?> GetSavingGoal(Guid savingGoalId, CancellationToken cancellationToken);
    Task<Guid> Add(Domain.Entites.MonthlySaving monthlySaving, CancellationToken cancellationToken);
}

public sealed class CreateMonthlySavingRepository(FinanceOneDbContext context) : ICreateMonthlySavingRepository
{
    public Task<SavingGoal?> GetSavingGoal(Guid savingGoalId, CancellationToken cancellationToken) =>
        context.SavingGoals.FirstOrDefaultAsync(s => s.Id == savingGoalId, cancellationToken);

    public async Task<Guid> Add(Domain.Entites.MonthlySaving monthlySaving, CancellationToken cancellationToken)
    {
        context.MonthlySavings.Add(monthlySaving);
        await context.SaveChangesAsync(cancellationToken);
        return monthlySaving.Id;
    }
}
