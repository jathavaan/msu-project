namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalsProjection;

/// <summary>One projected month: the sum of every saving goal's own running balance after that
/// month's contribution and compounded interest.</summary>
public sealed record SavingGoalsProjectionPointVm(int Month, DateOnly Date, decimal TotalBalance);
