using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.MonthlySavings.DeleteMonthlySaving;

public interface IDeleteMonthlySavingRepository
{
    Task<Domain.Entites.MonthlySaving?> GetById(Guid id, CancellationToken cancellationToken);
    Task Delete(Domain.Entites.MonthlySaving monthlySaving, CancellationToken cancellationToken);
}

public sealed class DeleteMonthlySavingRepository(FinanceOneDbContext context) : IDeleteMonthlySavingRepository
{
    public Task<Domain.Entites.MonthlySaving?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.MonthlySavings.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task Delete(Domain.Entites.MonthlySaving monthlySaving, CancellationToken cancellationToken)
    {
        context.MonthlySavings.Remove(monthlySaving);
        await context.SaveChangesAsync(cancellationToken);
    }
}
