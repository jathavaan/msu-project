namespace FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;

public sealed class DeleteSavingGoalHandler(IDeleteSavingGoalRepository repository)
    : IRequestHandler<DeleteSavingGoalCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var savingGoal = await repository.GetById(request.Id, cancellationToken);
        if (savingGoal is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Saving goal not found.");
        }

        await repository.Delete(savingGoal, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
