namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public sealed class CreateBudgetHandler(ICreateBudgetRepository repository)
    : IRequestHandler<CreateBudgetCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetExpenseCategory(request.CategoryId, cancellationToken);
        if (category is null || category.Type != CategoryType.Expense)
        {
            return Response<Guid>.Failure(StatusCodes.Status404NotFound, "Expense category not found.");
        }

        if (await repository.BudgetExistsForCategory(request.CategoryId, cancellationToken))
        {
            return Response<Guid>.Failure(StatusCodes.Status409Conflict, "A budget already exists for this category.");
        }

        var budget = new Budget { Id = Guid.NewGuid(), CategoryId = request.CategoryId, MonthlyLimit = request.MonthlyLimit };
        var id = await repository.Add(budget, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
