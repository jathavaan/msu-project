using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Income.GetIncomes;

public interface IGetIncomesRepository
{
    Task<List<IncomeVm>> GetIncomes(CancellationToken cancellationToken);
}

public sealed class GetIncomesRepository(FinanceOneDbContext context) : IGetIncomesRepository
{
    public Task<List<IncomeVm>> GetIncomes(CancellationToken cancellationToken) =>
        context.Incomes
            .OrderBy(i => i.Name)
            .Select(i => new IncomeVm(i.Id, i.Name, i.Amount, i.CategoryId, i.Category!.Name, i.RecurrenceDay))
            .ToListAsync(cancellationToken);
}
