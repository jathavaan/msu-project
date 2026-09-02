namespace FinanceOne.Api.Features.Expenses.UpdateExpense;

public sealed record UpdateExpenseCommand(Guid Id, string Name, decimal Amount, Guid CategoryId, int RecurrenceDay)
    : IRequest<Response<Unit>>;
