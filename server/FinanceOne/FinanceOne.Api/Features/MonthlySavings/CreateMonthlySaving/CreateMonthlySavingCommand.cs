namespace FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;

public sealed record CreateMonthlySavingCommand(string Name, decimal Amount, Guid SavingGoalId, int RecurrenceDay)
    : IRequest<Response<Guid>>;
