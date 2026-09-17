using FinanceOne.Api.Features.SavingGoals.GetSavingGoalsProjection;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.SavingGoals.GetSavingGoalsProjection;

public class GetSavingGoalsProjectionHandlerTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly IGetSavingGoalsProjectionRepository _repository = Substitute.For<IGetSavingGoalsProjectionRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetSavingGoalsProjectionHandler Handler => new(_repository, _timeProvider);

    private static SavingGoal AGoal(decimal currentAmount = 0m, decimal? interestRate = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "New Car",
        TargetAmount = 1_000_000m,
        CurrentAmount = currentAmount,
        TargetDate = Today.AddYears(2),
        InterestRate = interestRate,
    };

    private void Given(List<SavingGoal> goals, Dictionary<Guid, decimal>? contributions = null)
    {
        _repository.GetSavingGoals(Arg.Any<CancellationToken>()).Returns(goals);
        _repository.GetMonthlyContributionTotals(Arg.Any<CancellationToken>()).Returns(contributions ?? []);
    }

    [Fact]
    public async Task Reports_A_Flat_Zero_Series_When_There_Are_No_Goals()
    {
        Given([]);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: 1), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var points = response.Result!;
        Assert.Equal(13, points.Count); // month 0 through 12
        Assert.All(points, p => Assert.Equal(0m, p.TotalBalance));
    }

    [Fact]
    public async Task Defaults_To_Five_Years_When_Years_Is_Not_Provided()
    {
        Given([]);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: null), CancellationToken.None);

        Assert.Equal(61, response.Result!.Count); // 5 * 12 months + month 0
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Falls_Back_To_The_Default_Horizon_For_A_Non_Positive_Years(int years)
    {
        Given([]);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(years), CancellationToken.None);

        Assert.Equal(61, response.Result!.Count);
    }

    [Fact]
    public async Task Sums_Every_Goals_Balance_At_Each_Month()
    {
        var car = AGoal(currentAmount: 10_000m);
        var holiday = AGoal(currentAmount: 5_000m);
        Given(
            [car, holiday],
            new Dictionary<Guid, decimal> { [car.Id] = 1_000m, [holiday.Id] = 500m });

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: 1), CancellationToken.None);

        var points = response.Result!;
        Assert.Equal(15_000m, points[0].TotalBalance); // month 0: 10_000 + 5_000
        Assert.Equal(16_500m, points[1].TotalBalance); // month 1: (10_000+1_000) + (5_000+500)
    }

    // The whole point of the combined projection: a goal already past its own target keeps
    // compounding into the total instead of dropping out once GetSavingGoalProjection would
    // consider it "reached".
    [Fact]
    public async Task Keeps_Compounding_A_Goal_Past_Its_Own_Target()
    {
        var almostThere = AGoal(currentAmount: 900_000m, interestRate: 12m); // TargetAmount 1_000_000, 1%/month
        Given([almostThere]);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: 1), CancellationToken.None);

        var points = response.Result!;
        Assert.Equal(909_000m, points[1].TotalBalance); // 900_000 * 1.01
        // Keeps growing every month, including past the point it would cross TargetAmount, rather
        // than stopping like GetSavingGoalProjection does.
        Assert.True(points[^1].TotalBalance > 1_000_000m);
    }

    [Fact]
    public async Task Compounds_Each_Goals_Own_Interest_Rate_Independently()
    {
        var highYield = AGoal(currentAmount: 10_000m, interestRate: 12m); // 1%/month
        var noInterest = AGoal(currentAmount: 10_000m);
        Given([highYield, noInterest]);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: 1), CancellationToken.None);

        Assert.Equal(20_100m, response.Result![1].TotalBalance); // 10_100 + 10_000
    }

    [Fact]
    public async Task Each_Point_Has_The_Right_Month_And_Date()
    {
        Given([]);

        var response = await Handler.Handle(new GetSavingGoalsProjectionQuery(Years: 1), CancellationToken.None);

        var points = response.Result!;
        Assert.Equal(0, points[0].Month);
        Assert.Equal(Today, points[0].Date);
        Assert.Equal(3, points[3].Month);
        Assert.Equal(Today.AddMonths(3), points[3].Date);
    }
}
