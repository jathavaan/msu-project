namespace FinanceOne.Api.Features.Budgets.GetBudgetById;

public sealed record GetBudgetByIdQuery(Guid Id) : IRequest<Response<BudgetVm>>;
