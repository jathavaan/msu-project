using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Expenses.UpdateExpense;

public interface IUpdateExpenseRepository
{
    Task<Expense?> GetById(Guid id, CancellationToken cancellationToken);
    Task<Category?> GetExpenseCategory(Guid categoryId, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UpdateExpenseRepository(FinanceOneDbContext context) : IUpdateExpenseRepository
{
    public Task<Expense?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Expenses.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Category?> GetExpenseCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
