using FinanceOne.Api.Features.SavingGoals.GetSavingGoalsProjection;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.GetSavingGoalsProjection;

public class GetSavingGoalsProjectionTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetSavingGoalsProjectionHandler Handler =>
        new(new GetSavingGoalsProjectionRepository(Context), _timeProvider);

    [Fact]
    public async Task Reports_A_Flat_Zero_Series_When_There_Are_No_Goals()
    {
        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: 1), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var points = response.Result!;
        Assert.Equal(13, points.Count);
        Assert.All(points, p => Assert.Equal(0m, p.TotalBalance));
    }

    [Fact]
    public async Task Sums_Each_Goals_Balance_Using_Its_Own_Contributions_And_Interest()
    {
        var car = await GivenSavingGoal("New Car", 250_000m, Today.AddYears(2), currentAmount: 10_000m);
        var highYield = await GivenSavingGoal("High-Yield Savings", 1_000_000m, Today.AddYears(2), currentAmount: 10_000m, interestRate: 12m);
        await GivenMonthlySaving(car.Id, "Car Fund", 1_000m, 25);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: 1), CancellationToken.None);

        var points = response.Result!;
        Assert.Equal(20_000m, points[0].TotalBalance); // 10_000 + 10_000
        Assert.Equal(21_100m, points[1].TotalBalance); // (10_000 + 1_000) + (10_000 * 1.01)
    }

    // A goal already past its own TargetAmount keeps accumulating in the combined total instead of
    // dropping out — the behavior that distinguishes this slice from GetSavingGoalProjection.
    [Fact]
    public async Task Keeps_Compounding_A_Goal_Past_Its_Own_Target()
    {
        await GivenSavingGoal("New Car", 10_000m, Today.AddYears(2), currentAmount: 12_000m, interestRate: 12m);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: 1), CancellationToken.None);

        var points = response.Result!;
        Assert.Equal(12_120m, points[1].TotalBalance); // 12_000 * 1.01
        Assert.True(points[^1].TotalBalance > points[1].TotalBalance);
    }

    [Fact]
    public async Task Defaults_To_A_Five_Year_Horizon()
    {
        await GivenSavingGoal("New Car", 250_000m, Today.AddYears(2), currentAmount: 10_000m);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: null), CancellationToken.None);

        Assert.Equal(61, response.Result!.Count);
    }
}
