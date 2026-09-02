namespace FinanceOne.Api.Domain.Entites;

public sealed class Income
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
    public required Guid CategoryId { get; set; }
    public Category? Category { get; init; }

    // Day of month this income is received (1-28).
    public required int RecurrenceDay { get; set; }
}
