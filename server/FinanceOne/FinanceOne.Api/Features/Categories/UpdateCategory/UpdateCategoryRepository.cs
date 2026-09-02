using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Categories.UpdateCategory;

public interface IUpdateCategoryRepository
{
    Task<Category?> GetById(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsWithNameAndType(Guid excludingId, string name, CategoryType type, CancellationToken cancellationToken);
    Task Update(Category category, CancellationToken cancellationToken);
}

public sealed class UpdateCategoryRepository(FinanceOneDbContext context) : IUpdateCategoryRepository
{
    public Task<Category?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsWithNameAndType(Guid excludingId, string name, CategoryType type, CancellationToken cancellationToken) =>
        context.Categories.AnyAsync(c => c.Id != excludingId && c.Name == name && c.Type == type, cancellationToken);

    public Task Update(Category category, CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
