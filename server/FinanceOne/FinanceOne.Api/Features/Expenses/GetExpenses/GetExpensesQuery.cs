namespace FinanceOne.Api.Features.Expenses.GetExpenses;

public sealed record GetExpensesQuery(Guid? CategoryId) : IRequest<Response<List<ExpenseVm>>>;
