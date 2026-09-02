namespace FinanceOne.Api.Features.Expenses.GetExpenseById;

public sealed class GetExpenseByIdHandler(IGetExpenseByIdRepository repository)
    : IRequestHandler<GetExpenseByIdQuery, Response<ExpenseVm>>
{
    public async Task<Response<ExpenseVm>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await repository.GetById(request.Id, cancellationToken);
        return expense is null
            ? Response<ExpenseVm>.Failure(StatusCodes.Status404NotFound, "Expense not found.")
            : Response<ExpenseVm>.Success(expense);
    }
}
