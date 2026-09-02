namespace FinanceOne.Api.Features.Income.GetIncomeById;

public sealed class GetIncomeByIdHandler(IGetIncomeByIdRepository repository)
    : IRequestHandler<GetIncomeByIdQuery, Response<IncomeVm>>
{
    public async Task<Response<IncomeVm>> Handle(GetIncomeByIdQuery request, CancellationToken cancellationToken)
    {
        var income = await repository.GetById(request.Id, cancellationToken);
        return income is null
            ? Response<IncomeVm>.Failure(StatusCodes.Status404NotFound, "Income not found.")
            : Response<IncomeVm>.Success(income);
    }
}
