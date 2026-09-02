using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodes;

public interface IGetDiscountCodesRepository
{
    Task<List<DiscountCode>> GetDiscountCodes(int? expiringWithinDays, DateOnly today, CancellationToken cancellationToken);
}

public sealed class GetDiscountCodesRepository(FinanceOneDbContext context) : IGetDiscountCodesRepository
{
    public Task<List<DiscountCode>> GetDiscountCodes(int? expiringWithinDays, DateOnly today, CancellationToken cancellationToken)
    {
        var query = context.DiscountCodes.AsNoTracking().AsQueryable();

        if (expiringWithinDays is not null)
        {
            var cutoff = today.AddDays(expiringWithinDays.Value);
            query = query.Where(d => d.ExpiryDate >= today && d.ExpiryDate <= cutoff);
        }

        return query.OrderBy(d => d.ExpiryDate).ToListAsync(cancellationToken);
    }
}
