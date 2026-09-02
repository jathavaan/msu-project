namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoals;

public sealed class GetSavingGoalsHandler(IGetSavingGoalsRepository repository, TimeProvider timeProvider)
    : IRequestHandler<GetSavingGoalsQuery, Response<List<SavingGoalVm>>>
{
    public async Task<Response<List<SavingGoalVm>>> Handle(GetSavingGoalsQuery request, CancellationToken cancellationToken)
    {
        var savingGoals = await repository.GetSavingGoals(cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);

        var vms = savingGoals
            .Select(s => new SavingGoalVm(
                s.Id,
                s.Name,
                s.TargetAmount,
                s.TargetDate,
                s.CurrentAmount,
                s.TargetAmount - s.CurrentAmount,
                Math.Max(0, s.TargetDate.DayNumber - today.DayNumber)))
            .ToList();

        return Response<List<SavingGoalVm>>.Success(vms);
    }
}
