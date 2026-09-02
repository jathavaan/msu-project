namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;

public sealed class GetSavingGoalByIdHandler(IGetSavingGoalByIdRepository repository, TimeProvider timeProvider)
    : IRequestHandler<GetSavingGoalByIdQuery, Response<SavingGoalVm>>
{
    public async Task<Response<SavingGoalVm>> Handle(GetSavingGoalByIdQuery request, CancellationToken cancellationToken)
    {
        var savingGoal = await repository.GetById(request.Id, cancellationToken);
        if (savingGoal is null)
        {
            return Response<SavingGoalVm>.Failure(StatusCodes.Status404NotFound, "Saving goal not found.");
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var vm = new SavingGoalVm(
            savingGoal.Id,
            savingGoal.Name,
            savingGoal.TargetAmount,
            savingGoal.TargetDate,
            savingGoal.CurrentAmount,
            savingGoal.TargetAmount - savingGoal.CurrentAmount,
            Math.Max(0, savingGoal.TargetDate.DayNumber - today.DayNumber));

        return Response<SavingGoalVm>.Success(vm);
    }
}
