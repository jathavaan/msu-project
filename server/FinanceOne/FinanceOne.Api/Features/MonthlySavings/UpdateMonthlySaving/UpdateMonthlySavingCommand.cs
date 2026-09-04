namespace FinanceOne.Api.Features.MonthlySavings.UpdateMonthlySaving;

public sealed record UpdateMonthlySavingCommand(Guid Id, string Name, decimal Amount, Guid SavingGoalId, int RecurrenceDay)
    : IRequest<Response<Unit>>;
