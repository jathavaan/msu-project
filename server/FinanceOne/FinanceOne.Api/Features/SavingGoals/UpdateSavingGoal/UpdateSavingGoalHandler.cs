namespace FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;

public sealed class UpdateSavingGoalHandler(IUpdateSavingGoalRepository repository)
    : IRequestHandler<UpdateSavingGoalCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var savingGoal = await repository.GetById(request.Id, cancellationToken);
        if (savingGoal is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Saving goal not found.");
        }

        savingGoal.Name = request.Name;
        savingGoal.TargetAmount = request.TargetAmount;
        savingGoal.TargetDate = request.TargetDate;
        savingGoal.CurrentAmount = request.CurrentAmount;
        await repository.Update(cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
