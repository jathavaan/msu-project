using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Categories.CreateCategory;

public interface ICreateCategoryRepository
{
    Task<bool> ExistsWithNameAndType(string name, CategoryType type, CancellationToken cancellationToken);
    Task<Guid> Add(Category category, CancellationToken cancellationToken);
}

public sealed class CreateCategoryRepository(FinanceOneDbContext context) : ICreateCategoryRepository
{
    public Task<bool> ExistsWithNameAndType(string name, CategoryType type, CancellationToken cancellationToken) =>
        context.Categories.AnyAsync(c => c.Name == name && c.Type == type, cancellationToken);

    public async Task<Guid> Add(Category category, CancellationToken cancellationToken)
    {
        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
