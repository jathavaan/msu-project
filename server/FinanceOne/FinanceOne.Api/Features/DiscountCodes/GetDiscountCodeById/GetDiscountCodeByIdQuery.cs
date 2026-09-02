namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeById;

public sealed record GetDiscountCodeByIdQuery(Guid Id) : IRequest<Response<DiscountCode>>;
