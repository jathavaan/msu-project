namespace FinanceOne.Api.Features.Expenses.GetExpenseById;

public sealed record GetExpenseByIdQuery(Guid Id) : IRequest<Response<ExpenseVm>>;
