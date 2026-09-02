namespace FinanceOne.Api.Features.Expenses.UpdateExpense;

public sealed class UpdateExpenseHandler(IUpdateExpenseRepository repository)
    : IRequestHandler<UpdateExpenseCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await repository.GetById(request.Id, cancellationToken);
        if (expense is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Expense not found.");
        }

        var category = await repository.GetExpenseCategory(request.CategoryId, cancellationToken);
        if (category is null || category.Type != CategoryType.Expense)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Expense category not found.");
        }

        expense.Name = request.Name;
        expense.Amount = request.Amount;
        expense.CategoryId = request.CategoryId;
        expense.RecurrenceDay = request.RecurrenceDay;
        await repository.Update(cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
