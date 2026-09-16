namespace FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;

public sealed record CreateSavingGoalCommand(string Name, decimal TargetAmount, DateOnly TargetDate, decimal? InterestRate = null)
    : IRequest<Response<Guid>>;
