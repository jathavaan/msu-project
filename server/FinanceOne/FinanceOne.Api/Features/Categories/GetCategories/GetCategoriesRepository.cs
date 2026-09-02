using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Categories.GetCategories;

public interface IGetCategoriesRepository
{
    Task<List<CategoryVm>> GetCategories(CategoryType? type, CancellationToken cancellationToken);
}

public sealed class GetCategoriesRepository(FinanceOneDbContext context) : IGetCategoriesRepository
{
    public Task<List<CategoryVm>> GetCategories(CategoryType? type, CancellationToken cancellationToken) =>
        context.Categories
            .Where(c => type == null || c.Type == type)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryVm(c.Id, c.Name, c.Type))
            .ToListAsync(cancellationToken);
}
