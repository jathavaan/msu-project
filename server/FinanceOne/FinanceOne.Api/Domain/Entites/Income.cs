namespace FinanceOne.Api.Domain.Entites;

public sealed class Income
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Amount { get; init; }
    public required Guid CategoryId { get; init; }
    public Category? Category { get; init; }

    // Day of month this income is received (1-28).
    public required int RecurrenceDay { get; init; }
}
