namespace FinanceOne.Api.Features.Income.UpdateIncome;

public sealed record UpdateIncomeCommand(Guid Id, string Name, decimal Amount, Guid CategoryId, int RecurrenceDay)
    : IRequest<Response<Unit>>;
