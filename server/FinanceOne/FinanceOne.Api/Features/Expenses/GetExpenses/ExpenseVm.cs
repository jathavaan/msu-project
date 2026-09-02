namespace FinanceOne.Api.Features.Expenses.GetExpenses;

public sealed record ExpenseVm(Guid Id, string Name, decimal Amount, Guid CategoryId, string CategoryName, int RecurrenceDay);
