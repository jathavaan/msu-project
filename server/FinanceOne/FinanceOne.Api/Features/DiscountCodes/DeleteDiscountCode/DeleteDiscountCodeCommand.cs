namespace FinanceOne.Api.Features.DiscountCodes.DeleteDiscountCode;

public sealed record DeleteDiscountCodeCommand(Guid Id) : IRequest<Response<Unit>>;
