using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeImage;

public interface IGetDiscountCodeImageRepository
{
    Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetDiscountCodeImageRepository(FinanceOneDbContext context) : IGetDiscountCodeImageRepository
{
    public Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.DiscountCodes.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
}
