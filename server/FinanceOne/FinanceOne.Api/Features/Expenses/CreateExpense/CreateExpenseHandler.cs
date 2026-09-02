namespace FinanceOne.Api.Features.Expenses.CreateExpense;

public sealed class CreateExpenseHandler(ICreateExpenseRepository repository)
    : IRequestHandler<CreateExpenseCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetExpenseCategory(request.CategoryId, cancellationToken);
        if (category is null || category.Type != CategoryType.Expense)
        {
            return Response<Guid>.Failure(StatusCodes.Status404NotFound, "Expense category not found.");
        }

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Amount = request.Amount,
            CategoryId = request.CategoryId,
            RecurrenceDay = request.RecurrenceDay
        };
        var id = await repository.Add(expense, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
