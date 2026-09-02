using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Income.DeleteIncome;

public interface IDeleteIncomeRepository
{
    Task<Domain.Entites.Income?> GetById(Guid id, CancellationToken cancellationToken);
    Task Delete(Domain.Entites.Income income, CancellationToken cancellationToken);
}

public sealed class DeleteIncomeRepository(FinanceOneDbContext context) : IDeleteIncomeRepository
{
    public Task<Domain.Entites.Income?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Incomes.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task Delete(Domain.Entites.Income income, CancellationToken cancellationToken)
    {
        context.Incomes.Remove(income);
        await context.SaveChangesAsync(cancellationToken);
    }
}
