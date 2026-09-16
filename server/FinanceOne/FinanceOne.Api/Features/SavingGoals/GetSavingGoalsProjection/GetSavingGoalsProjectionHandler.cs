namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalsProjection;

public sealed class GetSavingGoalsProjectionHandler(IGetSavingGoalsProjectionRepository repository, TimeProvider timeProvider)
    : IRequestHandler<GetSavingGoalsProjectionQuery, Response<List<SavingGoalsProjectionPointVm>>>
{
    private const int DefaultYears = 5;

    public async Task<Response<List<SavingGoalsProjectionPointVm>>> Handle(GetSavingGoalsProjectionQuery request, CancellationToken cancellationToken)
    {
        var months = (request.Years is > 0 ? request.Years.Value : DefaultYears) * 12;

        var savingGoals = await repository.GetSavingGoals(cancellationToken);
        var monthlyContributionTotals = await repository.GetMonthlyContributionTotals(cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);

        // Each goal keeps compounding on its own balance/rate/contribution for the whole horizon —
        // unlike GetSavingGoalProjection, nothing stops early once a goal reaches its own target,
        // since a met goal should keep growing in the combined total rather than dropping out.
        var balances = savingGoals.Select(s => s.CurrentAmount).ToArray();
        var monthlyRates = savingGoals.Select(s => (s.InterestRate ?? 0m) / 100m / 12m).ToArray();
        var monthlyContributions = savingGoals.Select(s => monthlyContributionTotals.GetValueOrDefault(s.Id)).ToArray();

        var points = new List<SavingGoalsProjectionPointVm>(months + 1) { new(0, today, balances.Sum()) };

        for (var month = 1; month <= months; month++)
        {
            for (var i = 0; i < balances.Length; i++)
            {
                balances[i] = Math.Round(balances[i] + balances[i] * monthlyRates[i] + monthlyContributions[i], 2);
            }

            points.Add(new SavingGoalsProjectionPointVm(month, today.AddMonths(month), balances.Sum()));
        }

        return Response<List<SavingGoalsProjectionPointVm>>.Success(points);
    }
}
