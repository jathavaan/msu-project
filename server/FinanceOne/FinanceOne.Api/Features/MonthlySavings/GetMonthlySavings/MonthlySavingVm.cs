namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavings;

public sealed record MonthlySavingVm(Guid Id, string Name, decimal Amount, Guid SavingGoalId, string SavingGoalName, int RecurrenceDay);
