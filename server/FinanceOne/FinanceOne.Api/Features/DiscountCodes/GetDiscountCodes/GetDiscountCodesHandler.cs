namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodes;

public sealed class GetDiscountCodesHandler(IGetDiscountCodesRepository repository, TimeProvider timeProvider)
    : IRequestHandler<GetDiscountCodesQuery, Response<List<DiscountCode>>>
{
    public async Task<Response<List<DiscountCode>>> Handle(GetDiscountCodesQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var discountCodes = await repository.GetDiscountCodes(request.ExpiringWithinDays, today, cancellationToken);
        return Response<List<DiscountCode>>.Success(discountCodes);
    }
}
