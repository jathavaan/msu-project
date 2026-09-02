using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Expenses.DeleteExpense;

public interface IDeleteExpenseRepository
{
    Task<Expense?> GetById(Guid id, CancellationToken cancellationToken);
    Task Delete(Expense expense, CancellationToken cancellationToken);
}

public sealed class DeleteExpenseRepository(FinanceOneDbContext context) : IDeleteExpenseRepository
{
    public Task<Expense?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Expenses.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task Delete(Expense expense, CancellationToken cancellationToken)
    {
        context.Expenses.Remove(expense);
        await context.SaveChangesAsync(cancellationToken);
    }
}
