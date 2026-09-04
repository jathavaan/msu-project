using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavingById;

public interface IGetMonthlySavingByIdRepository
{
    Task<MonthlySavingVm?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetMonthlySavingByIdRepository(FinanceOneDbContext context) : IGetMonthlySavingByIdRepository
{
    public Task<MonthlySavingVm?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.MonthlySavings
            .Where(m => m.Id == id)
            .Select(m => new MonthlySavingVm(m.Id, m.Name, m.Amount, m.SavingGoalId, m.SavingGoal!.Name, m.RecurrenceDay))
            .FirstOrDefaultAsync(cancellationToken);
}
