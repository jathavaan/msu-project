namespace FinanceOne.Api.Features.Expenses.DeleteExpense;

public sealed record DeleteExpenseCommand(Guid Id) : IRequest<Response<Unit>>;
