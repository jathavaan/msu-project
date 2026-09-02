namespace FinanceOne.Api.Features.Income.CreateIncome;

public sealed record CreateIncomeCommand(string Name, decimal Amount, Guid CategoryId, int RecurrenceDay)
    : IRequest<Response<Guid>>;
