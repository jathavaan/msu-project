using FinanceOne.Api.Features.SavingGoals.GetSavingGoalProjection;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.GetSavingGoalProjection;

public class GetSavingGoalProjectionTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetSavingGoalProjectionHandler Handler =>
        new(new GetSavingGoalProjectionRepository(Context), _timeProvider);

    [Fact]
    public async Task Returns_404_When_The_Goal_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // Sums MonthlyContribution the same way GetSavingGoalById does — this exercises that the
    // projection's own repository query gets it right too.
    [Fact]
    public async Task Reaches_The_Target_Using_The_Sum_Of_Linked_Monthly_Savings()
    {
        var goal = await GivenSavingGoal("New Car", 15_000m, Today.AddYears(2), currentAmount: 10_000m);
        await GivenMonthlySaving(goal.Id, "Car Fund", 500m, 25);
        await GivenMonthlySaving(goal.Id, "Car Fund Extra", 500m, 10);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var vm = response.Result!;
        Assert.Equal(Today.AddMonths(5), vm.ReachDate);
        Assert.Equal(15_000m, vm.Points[^1].Balance);
    }

    [Fact]
    public async Task Compounds_The_Goals_Own_Interest_Rate_Monthly()
    {
        var goal = await GivenSavingGoal("High-Yield Savings", 10_100m, Today.AddYears(2), currentAmount: 10_000m, interestRate: 12m);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        var vm = response.Result!;
        Assert.Equal(Today.AddMonths(1), vm.ReachDate);
        Assert.Equal(10_100m, vm.Points[1].Balance);
    }

    [Fact]
    public async Task Reports_Unreachable_When_The_Goal_Has_No_Interest_Rate_Or_Contributions()
    {
        var goal = await GivenSavingGoal("New Car", 100_000m, Today.AddYears(2), currentAmount: 10_000m);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        Assert.Null(response.Result!.ReachDate);
    }

    [Fact]
    public async Task Already_Met_Goal_Reaches_Immediately()
    {
        var goal = await GivenSavingGoal("New Car", 10_000m, Today.AddYears(2), currentAmount: 12_000m);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        var vm = response.Result!;
        Assert.Equal(Today, vm.ReachDate);
        Assert.Equal(12_000m, Assert.Single(vm.Points).Balance);
    }

    // Only this goal's own monthly savings should feed its projection.
    [Fact]
    public async Task Ignores_Monthly_Savings_Belonging_To_Another_Goal()
    {
        var car = await GivenSavingGoal("New Car", 15_000m, Today.AddYears(2), currentAmount: 10_000m);
        var holiday = await GivenSavingGoal("Holiday", 30_000m, Today.AddYears(2), currentAmount: 0m);
        await GivenMonthlySaving(car.Id, "Car Fund", 1_000m, 25);
        await GivenMonthlySaving(holiday.Id, "Holiday Fund", 5_000m, 5);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(car.Id), CancellationToken.None);

        Assert.Equal(Today.AddMonths(5), response.Result!.ReachDate);
    }
}
