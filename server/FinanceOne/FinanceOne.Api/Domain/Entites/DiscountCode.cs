namespace FinanceOne.Api.Domain.Entites;

public sealed class DiscountCode
{
    public Guid Id { get; init; }
    public required string StoreName { get; set; }
    public string? CodeText { get; set; }

    public string? CodeImageUrl { get; set; }

    public required DateOnly ExpiryDate { get; set; }
}
