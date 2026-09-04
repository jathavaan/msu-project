using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.MonthlySavings.UpdateMonthlySaving;

public interface IUpdateMonthlySavingRepository
{
    Task<Domain.Entites.MonthlySaving?> GetById(Guid id, CancellationToken cancellationToken);
    Task<SavingGoal?> GetSavingGoal(Guid savingGoalId, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UpdateMonthlySavingRepository(FinanceOneDbContext context) : IUpdateMonthlySavingRepository
{
    public Task<Domain.Entites.MonthlySaving?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.MonthlySavings.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<SavingGoal?> GetSavingGoal(Guid savingGoalId, CancellationToken cancellationToken) =>
        context.SavingGoals.FirstOrDefaultAsync(s => s.Id == savingGoalId, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
