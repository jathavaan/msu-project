using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Expenses.GetExpenseById;

public interface IGetExpenseByIdRepository
{
    Task<ExpenseVm?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetExpenseByIdRepository(FinanceOneDbContext context) : IGetExpenseByIdRepository
{
    public Task<ExpenseVm?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Expenses
            .Where(e => e.Id == id)
            .Select(e => new ExpenseVm(e.Id, e.Name, e.Amount, e.CategoryId, e.Category!.Name, e.RecurrenceDay))
            .FirstOrDefaultAsync(cancellationToken);
}
