namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalsProjection;

public sealed record GetSavingGoalsProjectionQuery(int? Years) : IRequest<Response<List<SavingGoalsProjectionPointVm>>>;
