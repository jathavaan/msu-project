using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.DiscountCodes.UploadDiscountCodeImage;

public interface IUploadDiscountCodeImageRepository
{
    Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UploadDiscountCodeImageRepository(FinanceOneDbContext context) : IUploadDiscountCodeImageRepository
{
    public Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.DiscountCodes.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
