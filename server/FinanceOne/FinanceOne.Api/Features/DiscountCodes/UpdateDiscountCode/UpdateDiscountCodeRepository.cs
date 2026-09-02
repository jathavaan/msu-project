using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;

public interface IUpdateDiscountCodeRepository
{
    Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UpdateDiscountCodeRepository(FinanceOneDbContext context) : IUpdateDiscountCodeRepository
{
    public Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.DiscountCodes.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
