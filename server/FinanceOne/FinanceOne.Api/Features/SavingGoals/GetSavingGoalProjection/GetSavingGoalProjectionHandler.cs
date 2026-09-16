namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalProjection;

public sealed class GetSavingGoalProjectionHandler(IGetSavingGoalProjectionRepository repository, TimeProvider timeProvider)
    : IRequestHandler<GetSavingGoalProjectionQuery, Response<SavingGoalProjectionVm>>
{
    // 50 years — long enough to cover any realistic goal horizon without looping forever when the
    // rate at which the balance grows is too small (or zero) to ever reach TargetAmount.
    private const int MaxProjectionMonths = 600;

    public async Task<Response<SavingGoalProjectionVm>> Handle(GetSavingGoalProjectionQuery request, CancellationToken cancellationToken)
    {
        var savingGoal = await repository.GetById(request.Id, cancellationToken);
        if (savingGoal is null)
        {
            return Response<SavingGoalProjectionVm>.Failure(StatusCodes.Status404NotFound, "Saving goal not found.");
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var balance = savingGoal.CurrentAmount;
        var points = new List<SavingGoalProjectionPointVm> { new(0, today, balance) };

        if (balance >= savingGoal.TargetAmount)
        {
            return Response<SavingGoalProjectionVm>.Success(new SavingGoalProjectionVm(points, today));
        }

        var monthlyContribution = await repository.GetMonthlyContributionTotal(request.Id, cancellationToken);
        var monthlyRate = (savingGoal.InterestRate ?? 0m) / 100m / 12m;

        // Flat forever: nothing is added and nothing compounds, so the balance never moves from
        // where it started. Report unreachable immediately instead of grinding through
        // MaxProjectionMonths of an unchanged balance.
        if (monthlyContribution <= 0 && monthlyRate <= 0)
        {
            return Response<SavingGoalProjectionVm>.Success(new SavingGoalProjectionVm(points, null));
        }

        DateOnly? reachDate = null;
        for (var month = 1; month <= MaxProjectionMonths; month++)
        {
            balance = Math.Round(balance + balance * monthlyRate + monthlyContribution, 2);
            var date = today.AddMonths(month);
            points.Add(new SavingGoalProjectionPointVm(month, date, balance));

            if (balance >= savingGoal.TargetAmount)
            {
                reachDate = date;
                break;
            }
        }

        return Response<SavingGoalProjectionVm>.Success(new SavingGoalProjectionVm(points, reachDate));
    }
}
