using FinanceOne.Api.Features.SavingGoals.GetSavingGoalProjection;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.SavingGoals.GetSavingGoalProjection;

public class GetSavingGoalProjectionHandlerTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly IGetSavingGoalProjectionRepository _repository = Substitute.For<IGetSavingGoalProjectionRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetSavingGoalProjectionHandler Handler => new(_repository, _timeProvider);

    private static SavingGoal AGoal(
        decimal targetAmount = 100_000m,
        decimal currentAmount = 0m,
        decimal? interestRate = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "New Car",
        TargetAmount = targetAmount,
        CurrentAmount = currentAmount,
        TargetDate = Today.AddYears(2),
        InterestRate = interestRate,
    };

    [Fact]
    public async Task Returns_404_When_The_Goal_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SavingGoal?)null);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // Already at (or past) target from CurrentAmount alone — nothing left to project.
    [Fact]
    public async Task Already_At_Target_Reaches_Immediately_With_A_Single_Point()
    {
        var goal = AGoal(targetAmount: 100_000m, currentAmount: 120_000m);
        _repository.GetById(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var vm = response.Result!;
        Assert.Equal(Today, vm.ReachDate);
        var point = Assert.Single(vm.Points);
        Assert.Equal(0, point.Month);
        Assert.Equal(Today, point.Date);
        Assert.Equal(120_000m, point.Balance);
        await _repository.DidNotReceive().GetMonthlyContributionTotal(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // No contribution and no interest set — the balance can never move, so this must be reported
    // as unreachable immediately rather than generating a flat 600-point series.
    [Fact]
    public async Task Reports_Unreachable_When_Nothing_Grows_The_Balance()
    {
        var goal = AGoal(targetAmount: 100_000m, currentAmount: 10_000m);
        _repository.GetById(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);
        _repository.GetMonthlyContributionTotal(goal.Id, Arg.Any<CancellationToken>()).Returns(0m);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        var vm = response.Result!;
        Assert.Null(vm.ReachDate);
        var point = Assert.Single(vm.Points);
        Assert.Equal(10_000m, point.Balance);
    }

    [Fact]
    public async Task Projects_Contribution_Only_Growth_And_Finds_The_Reach_Month()
    {
        var goal = AGoal(targetAmount: 15_000m, currentAmount: 10_000m);
        _repository.GetById(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);
        _repository.GetMonthlyContributionTotal(goal.Id, Arg.Any<CancellationToken>()).Returns(1_000m);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        var vm = response.Result!;
        Assert.Equal(Today.AddMonths(5), vm.ReachDate);
        Assert.Equal(6, vm.Points.Count); // month 0 through month 5
        Assert.Equal(15_000m, vm.Points[^1].Balance);
    }

    // Interest alone (no monthly contribution) still compounds the balance forward.
    [Fact]
    public async Task Projects_Interest_Only_Growth()
    {
        var goal = AGoal(targetAmount: 10_100m, currentAmount: 10_000m, interestRate: 12m); // 1%/month
        _repository.GetById(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);
        _repository.GetMonthlyContributionTotal(goal.Id, Arg.Any<CancellationToken>()).Returns(0m);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        var vm = response.Result!;
        Assert.NotNull(vm.ReachDate);
        Assert.Equal(10_100m, vm.Points[1].Balance); // 10_000 * 1.01
    }

    // The projection stops climbing once TargetAmount is met, rather than continuing to the
    // 600-month cap — the chart has no use for balance past the point the goal is reached.
    [Fact]
    public async Task Stops_Generating_Points_Once_The_Target_Is_Reached()
    {
        var goal = AGoal(targetAmount: 12_000m, currentAmount: 10_000m);
        _repository.GetById(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);
        _repository.GetMonthlyContributionTotal(goal.Id, Arg.Any<CancellationToken>()).Returns(2_000m);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        var vm = response.Result!;
        Assert.Equal(Today.AddMonths(1), vm.ReachDate);
        Assert.Equal(2, vm.Points.Count);
    }

    // A rate too small to close the gap within the 600-month (50-year) horizon is treated as
    // unreachable rather than projected further.
    [Fact]
    public async Task Reports_Unreachable_When_The_Rate_Is_Too_Slow_For_The_Horizon()
    {
        var goal = AGoal(targetAmount: 1_000_000m, currentAmount: 0m);
        _repository.GetById(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);
        _repository.GetMonthlyContributionTotal(goal.Id, Arg.Any<CancellationToken>()).Returns(1m);

        var response = await Handler.Handle(new GetSavingGoalProjectionQuery(goal.Id), CancellationToken.None);

        var vm = response.Result!;
        Assert.Null(vm.ReachDate);
        Assert.Equal(601, vm.Points.Count); // month 0 through the 600-month cap
    }
}
