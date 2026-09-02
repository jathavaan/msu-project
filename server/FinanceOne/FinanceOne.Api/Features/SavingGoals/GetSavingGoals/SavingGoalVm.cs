namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoals;

public sealed record SavingGoalVm(
    Guid Id,
    string Name,
    decimal TargetAmount,
    DateOnly TargetDate,
    decimal AmountSaved,
    decimal AmountRemaining,
    int DaysRemaining);
