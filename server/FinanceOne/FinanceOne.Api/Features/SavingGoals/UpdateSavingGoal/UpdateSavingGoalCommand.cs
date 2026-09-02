namespace FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;

public sealed record UpdateSavingGoalCommand(Guid Id, string Name, decimal TargetAmount, DateOnly TargetDate, decimal CurrentAmount)
    : IRequest<Response<Unit>>;
