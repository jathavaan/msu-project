using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalsProjection;

public interface IGetSavingGoalsProjectionRepository
{
    Task<List<SavingGoal>> GetSavingGoals(CancellationToken cancellationToken);

    // Sum of MonthlySaving.Amount per goal — same shape as GetSavingGoalsRepository's own query,
    // duplicated here per this codebase's one-repository-per-slice convention.
    Task<Dictionary<Guid, decimal>> GetMonthlyContributionTotals(CancellationToken cancellationToken);
}

public sealed class GetSavingGoalsProjectionRepository(FinanceOneDbContext context) : IGetSavingGoalsProjectionRepository
{
    public Task<List<SavingGoal>> GetSavingGoals(CancellationToken cancellationToken) =>
        context.SavingGoals.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Dictionary<Guid, decimal>> GetMonthlyContributionTotals(CancellationToken cancellationToken) =>
        context.MonthlySavings
            .GroupBy(m => m.SavingGoalId)
            .Select(g => new { SavingGoalId = g.Key, Total = g.Sum(m => m.Amount) })
            .ToDictionaryAsync(x => x.SavingGoalId, x => x.Total, cancellationToken);
}
