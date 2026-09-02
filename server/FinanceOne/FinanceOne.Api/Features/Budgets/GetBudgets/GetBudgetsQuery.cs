namespace FinanceOne.Api.Features.Budgets.GetBudgets;

public sealed record GetBudgetsQuery : IRequest<Response<List<BudgetVm>>>;
