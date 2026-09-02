namespace FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;

public sealed record CreateDiscountCodeCommand(string StoreName, string? CodeText, string? CodeImageUrl, DateOnly ExpiryDate)
    : IRequest<Response<Guid>>;
