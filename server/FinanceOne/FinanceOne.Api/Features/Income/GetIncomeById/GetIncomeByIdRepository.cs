using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Income.GetIncomeById;

public interface IGetIncomeByIdRepository
{
    Task<IncomeVm?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetIncomeByIdRepository(FinanceOneDbContext context) : IGetIncomeByIdRepository
{
    public Task<IncomeVm?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Incomes
            .Where(i => i.Id == id)
            .Select(i => new IncomeVm(i.Id, i.Name, i.Amount, i.CategoryId, i.Category!.Name, i.RecurrenceDay))
            .FirstOrDefaultAsync(cancellationToken);
}
