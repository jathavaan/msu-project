namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavingById;

public sealed record GetMonthlySavingByIdQuery(Guid Id) : IRequest<Response<MonthlySavingVm>>;
