using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Categories.GetCategoryById;

public interface IGetCategoryByIdRepository
{
    Task<CategoryVm?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetCategoryByIdRepository(FinanceOneDbContext context) : IGetCategoryByIdRepository
{
    public Task<CategoryVm?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryVm(c.Id, c.Name, c.Type))
            .FirstOrDefaultAsync(cancellationToken);
}
