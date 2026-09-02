namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodes;

public sealed record GetDiscountCodesQuery(int? ExpiringWithinDays) : IRequest<Response<List<DiscountCode>>>;
