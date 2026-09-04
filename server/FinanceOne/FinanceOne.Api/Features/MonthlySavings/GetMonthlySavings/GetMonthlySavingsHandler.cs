namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavings;

public sealed class GetMonthlySavingsHandler(IGetMonthlySavingsRepository repository)
    : IRequestHandler<GetMonthlySavingsQuery, Response<List<MonthlySavingVm>>>
{
    public async Task<Response<List<MonthlySavingVm>>> Handle(GetMonthlySavingsQuery request, CancellationToken cancellationToken)
    {
        var monthlySavings = await repository.GetMonthlySavings(cancellationToken);
        return Response<List<MonthlySavingVm>>.Success(monthlySavings);
    }
}
