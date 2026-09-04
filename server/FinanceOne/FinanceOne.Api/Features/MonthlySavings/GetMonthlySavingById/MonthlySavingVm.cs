namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavingById;

public sealed record MonthlySavingVm(Guid Id, string Name, decimal Amount, Guid SavingGoalId, string SavingGoalName, int RecurrenceDay);
