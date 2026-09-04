using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoals;

public interface IGetSavingGoalsRepository
{
    Task<List<SavingGoal>> GetSavingGoals(CancellationToken cancellationToken);

    // Sum of MonthlySaving.Amount per goal — MonthlySavings is a table SavingGoals doesn't
    // own, keyed by SavingGoalId so the handler can look up each goal's total in one pass.
    Task<Dictionary<Guid, decimal>> GetMonthlyContributionTotals(CancellationToken cancellationToken);
}

public sealed class GetSavingGoalsRepository(FinanceOneDbContext context) : IGetSavingGoalsRepository
{
    public Task<List<SavingGoal>> GetSavingGoals(CancellationToken cancellationToken) =>
        context.SavingGoals.AsNoTracking().OrderBy(s => s.TargetDate).ToListAsync(cancellationToken);

    public Task<Dictionary<Guid, decimal>> GetMonthlyContributionTotals(CancellationToken cancellationToken) =>
        context.MonthlySavings
            .GroupBy(m => m.SavingGoalId)
            .Select(g => new { SavingGoalId = g.Key, Total = g.Sum(m => m.Amount) })
            .ToDictionaryAsync(x => x.SavingGoalId, x => x.Total, cancellationToken);
}
