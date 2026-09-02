namespace FinanceOne.Api.Features.Income.GetIncomes;

public sealed class GetIncomesHandler(IGetIncomesRepository repository)
    : IRequestHandler<GetIncomesQuery, Response<List<IncomeVm>>>
{
    public async Task<Response<List<IncomeVm>>> Handle(GetIncomesQuery request, CancellationToken cancellationToken)
    {
        var incomes = await repository.GetIncomes(cancellationToken);
        return Response<List<IncomeVm>>.Success(incomes);
    }
}
