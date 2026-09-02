namespace FinanceOne.Api.Features.Budgets.GetBudgetById;

public sealed class GetBudgetByIdHandler(IGetBudgetByIdRepository repository)
    : IRequestHandler<GetBudgetByIdQuery, Response<BudgetVm>>
{
    public async Task<Response<BudgetVm>> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
    {
        var budget = await repository.GetById(request.Id, cancellationToken);
        return budget is null
            ? Response<BudgetVm>.Failure(StatusCodes.Status404NotFound, "Budget not found.")
            : Response<BudgetVm>.Success(budget);
    }
}
