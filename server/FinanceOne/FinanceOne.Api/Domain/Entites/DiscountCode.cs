namespace FinanceOne.Api.Domain.Entites;

public sealed class DiscountCode
{
    public Guid Id { get; init; }
    public required string StoreName { get; init; }
    public string? CodeText { get; init; }

    public string? CodeImageUrl { get; init; }

    public required DateOnly ExpiryDate { get; init; }
}
