using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Categories.DeleteCategory;

public interface IDeleteCategoryRepository
{
    Task<Category?> GetById(Guid id, CancellationToken cancellationToken);
    Task<bool> IsReferenced(Guid categoryId, CancellationToken cancellationToken);
    Task Delete(Category category, CancellationToken cancellationToken);
}

public sealed class DeleteCategoryRepository(FinanceOneDbContext context) : IDeleteCategoryRepository
{
    public Task<Category?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> IsReferenced(Guid categoryId, CancellationToken cancellationToken)
    {
        if (await context.Incomes.AnyAsync(i => i.CategoryId == categoryId, cancellationToken))
        {
            return true;
        }

        if (await context.Expenses.AnyAsync(e => e.CategoryId == categoryId, cancellationToken))
        {
            return true;
        }

        return await context.Budgets.AnyAsync(b => b.CategoryId == categoryId, cancellationToken);
    }

    public async Task Delete(Category category, CancellationToken cancellationToken)
    {
        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
    }
}
