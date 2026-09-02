namespace FinanceOne.Api.Features.Expenses.CreateExpense;

public sealed record CreateExpenseCommand(string Name, decimal Amount, Guid CategoryId, int RecurrenceDay)
    : IRequest<Response<Guid>>;
