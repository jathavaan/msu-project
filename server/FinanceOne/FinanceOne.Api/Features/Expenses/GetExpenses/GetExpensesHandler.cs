namespace FinanceOne.Api.Features.Expenses.GetExpenses;

public sealed class GetExpensesHandler(IGetExpensesRepository repository)
    : IRequestHandler<GetExpensesQuery, Response<List<ExpenseVm>>>
{
    public async Task<Response<List<ExpenseVm>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var expenses = await repository.GetExpenses(request.CategoryId, cancellationToken);
        return Response<List<ExpenseVm>>.Success(expenses);
    }
}
