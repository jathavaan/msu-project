namespace FinanceOne.Api.Features.Budgets.DeleteBudget;

public sealed record DeleteBudgetCommand(Guid Id) : IRequest<Response<Unit>>;
