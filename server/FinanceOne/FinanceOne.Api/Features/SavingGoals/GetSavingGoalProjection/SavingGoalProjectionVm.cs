namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalProjection;

/// <summary>One projected month: the running balance after that month's contribution and compounded interest.</summary>
public sealed record SavingGoalProjectionPointVm(int Month, DateOnly Date, decimal Balance);

/// <summary>
/// <see cref="ReachDate"/> is null when the goal is never projected to reach <c>TargetAmount</c> —
/// either because contribution and interest are both 0, or because the projection horizon
/// (see <c>GetSavingGoalProjectionHandler.MaxProjectionMonths</c>) ran out first.
/// </summary>
public sealed record SavingGoalProjectionVm(List<SavingGoalProjectionPointVm> Points, DateOnly? ReachDate);
