namespace FinanceOne.Api.Features.Budgets.GetBudgets;

public sealed class GetBudgetsHandler(IGetBudgetsRepository repository)
    : IRequestHandler<GetBudgetsQuery, Response<List<BudgetVm>>>
{
    public async Task<Response<List<BudgetVm>>> Handle(GetBudgetsQuery request, CancellationToken cancellationToken)
    {
        var budgets = await repository.GetBudgetsWithUsage(cancellationToken);
        return Response<List<BudgetVm>>.Success(budgets);
    }
}
