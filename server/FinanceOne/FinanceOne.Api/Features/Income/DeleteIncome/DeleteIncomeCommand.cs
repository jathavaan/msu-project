namespace FinanceOne.Api.Features.Income.DeleteIncome;

public sealed record DeleteIncomeCommand(Guid Id) : IRequest<Response<Unit>>;
