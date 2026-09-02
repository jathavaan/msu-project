using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.DiscountCodes.DeleteDiscountCode;

public interface IDeleteDiscountCodeRepository
{
    Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken);
    Task Delete(DiscountCode discountCode, CancellationToken cancellationToken);
}

public sealed class DeleteDiscountCodeRepository(FinanceOneDbContext context) : IDeleteDiscountCodeRepository
{
    public Task<DiscountCode?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.DiscountCodes.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task Delete(DiscountCode discountCode, CancellationToken cancellationToken)
    {
        context.DiscountCodes.Remove(discountCode);
        await context.SaveChangesAsync(cancellationToken);
    }
}
