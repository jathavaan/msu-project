namespace FinanceOne.Api.Domain.Entites;

public sealed class SavingGoal
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal TargetAmount { get; init; }
    public required DateOnly TargetDate { get; init; }
}
