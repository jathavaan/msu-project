namespace FinanceOne.Api.Features.MonthlySavings.DeleteMonthlySaving;

public sealed record DeleteMonthlySavingCommand(Guid Id) : IRequest<Response<Unit>>;
