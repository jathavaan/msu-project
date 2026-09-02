namespace FinanceOne.Api.Features.Budgets.DeleteBudget;

public sealed class DeleteBudgetHandler(IDeleteBudgetRepository repository)
    : IRequestHandler<DeleteBudgetCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await repository.GetById(request.Id, cancellationToken);
        if (budget is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Budget not found.");
        }

        await repository.Delete(budget, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
