namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;

public sealed record SavingGoalVm(
    Guid Id,
    string Name,
    decimal TargetAmount,
    DateOnly TargetDate,
    decimal AmountSaved,
    decimal AmountRemaining,
    int DaysRemaining,
    decimal MonthlyContribution);
