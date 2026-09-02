namespace FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;

public sealed record CreateSavingGoalCommand(string Name, decimal TargetAmount, DateOnly TargetDate)
    : IRequest<Response<Guid>>;
