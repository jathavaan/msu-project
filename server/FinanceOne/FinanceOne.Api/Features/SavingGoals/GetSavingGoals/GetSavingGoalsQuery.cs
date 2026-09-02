namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoals;

public sealed record GetSavingGoalsQuery : IRequest<Response<List<SavingGoalVm>>>;
