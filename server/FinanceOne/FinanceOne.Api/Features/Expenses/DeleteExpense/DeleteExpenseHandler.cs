namespace FinanceOne.Api.Features.Expenses.DeleteExpense;

public sealed class DeleteExpenseHandler(IDeleteExpenseRepository repository)
    : IRequestHandler<DeleteExpenseCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await repository.GetById(request.Id, cancellationToken);
        if (expense is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Expense not found.");
        }

        await repository.Delete(expense, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
