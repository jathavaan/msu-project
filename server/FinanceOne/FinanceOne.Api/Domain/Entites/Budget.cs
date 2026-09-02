namespace FinanceOne.Api.Domain.Entites;

public sealed class Budget
{
    public Guid Id { get; init; }

    public required Guid CategoryId { get; init; }
    public Category? Category { get; init; }

    public required decimal MonthlyLimit { get; set; }
}
