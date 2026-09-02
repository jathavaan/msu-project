using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Expenses.GetExpenses;

public interface IGetExpensesRepository
{
    Task<List<ExpenseVm>> GetExpenses(Guid? categoryId, CancellationToken cancellationToken);
}

public sealed class GetExpensesRepository(FinanceOneDbContext context) : IGetExpensesRepository
{
    public Task<List<ExpenseVm>> GetExpenses(Guid? categoryId, CancellationToken cancellationToken) =>
        context.Expenses
            .Where(e => categoryId == null || e.CategoryId == categoryId)
            .OrderBy(e => e.Name)
            .Select(e => new ExpenseVm(e.Id, e.Name, e.Amount, e.CategoryId, e.Category!.Name, e.RecurrenceDay))
            .ToListAsync(cancellationToken);
}
