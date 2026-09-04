using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavings;

public interface IGetMonthlySavingsRepository
{
    Task<List<MonthlySavingVm>> GetMonthlySavings(CancellationToken cancellationToken);
}

public sealed class GetMonthlySavingsRepository(FinanceOneDbContext context) : IGetMonthlySavingsRepository
{
    public Task<List<MonthlySavingVm>> GetMonthlySavings(CancellationToken cancellationToken) =>
        context.MonthlySavings
            .OrderBy(m => m.Name)
            .Select(m => new MonthlySavingVm(m.Id, m.Name, m.Amount, m.SavingGoalId, m.SavingGoal!.Name, m.RecurrenceDay))
            .ToListAsync(cancellationToken);
}
