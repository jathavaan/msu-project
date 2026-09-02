namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public sealed record CreateBudgetCommand(Guid CategoryId, decimal MonthlyLimit) : IRequest<Response<Guid>>;
