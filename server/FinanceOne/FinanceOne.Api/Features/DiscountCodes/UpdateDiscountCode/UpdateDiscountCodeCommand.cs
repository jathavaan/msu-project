namespace FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;

public sealed record UpdateDiscountCodeCommand(Guid Id, string StoreName, string? CodeText, string? CodeImageUrl, DateOnly ExpiryDate)
    : IRequest<Response<Unit>>;
