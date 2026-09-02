namespace FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;

public interface ICreateDiscountCodeRepository
{
    Task<Guid> Add(DiscountCode discountCode, CancellationToken cancellationToken);
}

public sealed class CreateDiscountCodeRepository(FinanceOneDbContext context) : ICreateDiscountCodeRepository
{
    public async Task<Guid> Add(DiscountCode discountCode, CancellationToken cancellationToken)
    {
        context.DiscountCodes.Add(discountCode);
        await context.SaveChangesAsync(cancellationToken);
        return discountCode.Id;
    }
}
