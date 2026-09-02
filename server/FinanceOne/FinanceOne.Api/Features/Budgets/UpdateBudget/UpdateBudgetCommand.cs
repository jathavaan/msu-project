namespace FinanceOne.Api.Features.Budgets.UpdateBudget;

public sealed record UpdateBudgetCommand(Guid Id, decimal MonthlyLimit) : IRequest<Response<Unit>>;
