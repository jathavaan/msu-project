namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;

public sealed record GetSavingGoalByIdQuery(Guid Id) : IRequest<Response<SavingGoalVm>>;
