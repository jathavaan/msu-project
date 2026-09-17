using FinanceOne.Api.Features.SavingGoals.GetSavingGoals;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.SavingGoals.GetSavingGoals;

// Unusually for this codebase, this handler does the arithmetic itself rather than delegating to
// the repository: AmountRemaining, DaysRemaining and the per-goal contribution lookup are all
// computed in Handle. That makes it the most valuable handler in the project to unit test.
public class GetSavingGoalsHandlerTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly IGetSavingGoalsRepository _repository = Substitute.For<IGetSavingGoalsRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetSavingGoalsHandler Handler => new(_repository, _timeProvider);

    private static SavingGoal AGoal(
        Guid id,
        decimal targetAmount = 100_000m,
        decimal currentAmount = 0m,
        DateOnly? targetDate = null,
        decimal? interestRate = null) => new()
    {
        Id = id,
        Name = "New Car",
        TargetAmount = targetAmount,
        CurrentAmount = currentAmount,
        TargetDate = targetDate ?? Today.AddDays(30),
        InterestRate = interestRate,
    };

    private void Given(List<SavingGoal> goals, Dictionary<Guid, decimal>? contributions = null)
    {
        _repository.GetSavingGoals(Arg.Any<CancellationToken>()).Returns(goals);
        _repository.GetMonthlyContributionTotals(Arg.Any<CancellationToken>()).Returns(contributions ?? []);
    }

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_No_Goals()
    {
        Given([]);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Computes_Amount_Remaining()
    {
        Given([AGoal(Guid.NewGuid(), targetAmount: 250_000m, currentAmount: 80_000m)]);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        var vm = Assert.Single(response.Result!);
        Assert.Equal(80_000m, vm.AmountSaved);
        Assert.Equal(170_000m, vm.AmountRemaining);
    }

    // Overshooting the target is allowed (CurrentAmount is adjusted by hand), and the remainder
    // goes negative rather than clamping — the frontend renders that as "goal exceeded".
    [Fact]
    public async Task Amount_Remaining_Goes_Negative_When_The_Goal_Is_Exceeded()
    {
        Given([AGoal(Guid.NewGuid(), targetAmount: 100_000m, currentAmount: 120_000m)]);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(-20_000m, Assert.Single(response.Result!).AmountRemaining);
    }

    [Fact]
    public async Task Computes_Days_Remaining_From_Today()
    {
        Given([AGoal(Guid.NewGuid(), targetDate: Today.AddDays(45))]);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(45, Assert.Single(response.Result!).DaysRemaining);
    }

    // DaysRemaining is clamped at zero, so an overdue goal reads "0 days left" rather than a
    // negative countdown.
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(-90, 0)]
    public async Task Days_Remaining_Never_Goes_Negative(int daysFromToday, int expected)
    {
        Given([AGoal(Guid.NewGuid(), targetDate: Today.AddDays(daysFromToday))]);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(expected, Assert.Single(response.Result!).DaysRemaining);
    }

    [Fact]
    public async Task Attaches_Each_Goals_Own_Monthly_Contribution_Total()
    {
        var car = Guid.NewGuid();
        var holiday = Guid.NewGuid();
        Given(
            [AGoal(car), AGoal(holiday)],
            new Dictionary<Guid, decimal> { [car] = 5_000m, [holiday] = 1_500m });

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(5_000m, response.Result!.Single(v => v.Id == car).MonthlyContribution);
        Assert.Equal(1_500m, response.Result!.Single(v => v.Id == holiday).MonthlyContribution);
    }

    // A goal nothing contributes to has no row in the totals dictionary at all, so the lookup must
    // fall back to zero rather than throwing.
    [Fact]
    public async Task Reports_Zero_Contribution_For_A_Goal_With_No_Monthly_Savings()
    {
        var id = Guid.NewGuid();
        Given([AGoal(id)], new Dictionary<Guid, decimal> { [Guid.NewGuid()] = 5_000m });

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(0m, Assert.Single(response.Result!).MonthlyContribution);
    }

    [Fact]
    public async Task Carries_The_Goals_Interest_Rate_Through_Unchanged()
    {
        Given([AGoal(Guid.NewGuid(), interestRate: 4.5m)]);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(4.5m, Assert.Single(response.Result!).InterestRate);
    }

    [Fact]
    public async Task Reports_Null_Interest_Rate_When_The_Goal_Has_None()
    {
        Given([AGoal(Guid.NewGuid())]);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Null(Assert.Single(response.Result!).InterestRate);
    }
}
