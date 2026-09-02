namespace FinanceOne.Api.Features.Income.GetIncomes;

public sealed record IncomeVm(Guid Id, string Name, decimal Amount, Guid CategoryId, string CategoryName, int RecurrenceDay);
