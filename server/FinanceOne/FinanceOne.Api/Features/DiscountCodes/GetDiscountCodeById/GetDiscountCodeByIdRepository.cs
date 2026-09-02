using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeById;

public interface IGetDiscountCodeByIdRepository
{
    Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken);
}

public sealed class GetDiscountCodeByIdRepository(FinanceOneDbContext context) : IGetDiscountCodeByIdRepository
{
    public Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.DiscountCodes.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
}
