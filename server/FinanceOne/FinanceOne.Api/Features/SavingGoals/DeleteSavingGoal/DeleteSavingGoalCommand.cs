namespace FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;

public sealed record DeleteSavingGoalCommand(Guid Id) : IRequest<Response<Unit>>;
