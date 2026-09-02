namespace FinanceOne.Api.Features.Expenses.GetExpenseById;

public sealed record ExpenseVm(Guid Id, string Name, decimal Amount, Guid CategoryId, string CategoryName, int RecurrenceDay);
