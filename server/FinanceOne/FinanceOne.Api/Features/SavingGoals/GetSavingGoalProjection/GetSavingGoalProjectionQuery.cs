namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalProjection;

public sealed record GetSavingGoalProjectionQuery(Guid Id) : IRequest<Response<SavingGoalProjectionVm>>;
