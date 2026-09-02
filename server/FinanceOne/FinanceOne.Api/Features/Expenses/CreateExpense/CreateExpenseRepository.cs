using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Expenses.CreateExpense;

public interface ICreateExpenseRepository
{
    Task<Category?> GetExpenseCategory(Guid categoryId, CancellationToken cancellationToken);
    Task<Guid> Add(Expense expense, CancellationToken cancellationToken);
}

public sealed class CreateExpenseRepository(FinanceOneDbContext context) : ICreateExpenseRepository
{
    public Task<Category?> GetExpenseCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public async Task<Guid> Add(Expense expense, CancellationToken cancellationToken)
    {
        context.Expenses.Add(expense);
        await context.SaveChangesAsync(cancellationToken);
        return expense.Id;
    }
}
