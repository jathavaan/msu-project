using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;

public interface IGetSavingGoalByIdRepository
{
    Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken);
    Task<decimal> GetMonthlyContributionTotal(Guid savingGoalId, CancellationToken cancellationToken);
}

public sealed class GetSavingGoalByIdRepository(FinanceOneDbContext context) : IGetSavingGoalByIdRepository
{
    public Task<SavingGoal?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.SavingGoals.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<decimal> GetMonthlyContributionTotal(Guid savingGoalId, CancellationToken cancellationToken) =>
        await context.MonthlySavings
            .Where(m => m.SavingGoalId == savingGoalId)
            .SumAsync(m => (decimal?)m.Amount, cancellationToken) ?? 0m;
}
