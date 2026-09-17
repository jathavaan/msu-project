using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Transactions.UpdateTransactionCategory;

public interface IUpdateTransactionCategoryRepository
{
    Task<Transaction?> GetById(Guid id, CancellationToken cancellationToken);
    Task<Category?> GetCategory(Guid categoryId, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UpdateTransactionCategoryRepository(FinanceOneDbContext context) : IUpdateTransactionCategoryRepository
{
    public Task<Transaction?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Transactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Category?> GetCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
