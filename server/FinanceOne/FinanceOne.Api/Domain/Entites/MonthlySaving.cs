namespace FinanceOne.Api.Domain.Entites;

// A recurring monthly amount set aside towards a SavingGoal — the mechanism by which
// SavingGoal.CurrentAmount is meant to grow over time (still adjusted manually via
// UpdateSavingGoal for now; see Features/SavingGoals/GetSavingGoals's README).
public sealed class MonthlySaving
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
    public required Guid SavingGoalId { get; set; }
    public SavingGoal? SavingGoal { get; init; }

    // Day of month this amount is set aside (1-28).
    public required int RecurrenceDay { get; set; }
}
