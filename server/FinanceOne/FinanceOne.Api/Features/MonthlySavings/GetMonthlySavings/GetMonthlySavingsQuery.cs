namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavings;

public sealed record GetMonthlySavingsQuery : IRequest<Response<List<MonthlySavingVm>>>;
