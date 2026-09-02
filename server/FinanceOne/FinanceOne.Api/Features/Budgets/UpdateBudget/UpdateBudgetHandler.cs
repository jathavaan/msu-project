namespace FinanceOne.Api.Features.Budgets.UpdateBudget;

public sealed class UpdateBudgetHandler(IUpdateBudgetRepository repository)
    : IRequestHandler<UpdateBudgetCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await repository.GetById(request.Id, cancellationToken);
        if (budget is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Budget not found.");
        }

        budget.MonthlyLimit = request.MonthlyLimit;
        await repository.Update(cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
