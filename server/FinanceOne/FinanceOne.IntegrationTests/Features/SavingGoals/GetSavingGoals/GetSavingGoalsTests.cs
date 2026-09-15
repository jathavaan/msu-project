using FinanceOne.Api.Features.SavingGoals.GetSavingGoals;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.GetSavingGoals;

public class GetSavingGoalsTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetSavingGoalsHandler Handler => new(new GetSavingGoalsRepository(Context), _timeProvider);

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_No_Goals()
    {
        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    // Goals are listed by deadline so the most urgent one leads, which is what the dashboard
    // widget relies on.
    [Fact]
    public async Task Orders_By_Target_Date()
    {
        await GivenSavingGoal("New Car", 250_000m, Today.AddYears(2));
        await GivenSavingGoal("Holiday", 30_000m, Today.AddMonths(3));
        await GivenSavingGoal("Laptop", 25_000m, Today.AddYears(1));

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(["Holiday", "Laptop", "New Car"], response.Result!.Select(s => s.Name));
    }

    // The contribution total is a GROUP BY over MonthlySavings keyed by goal, so this checks the
    // grouping attributes each sum to the right goal rather than pooling them.
    [Fact]
    public async Task Sums_Each_Goals_Monthly_Contributions_Separately()
    {
        var car = await GivenSavingGoal("New Car", 250_000m, Today.AddYears(1));
        var holiday = await GivenSavingGoal("Holiday", 30_000m, Today.AddYears(2));
        await GivenMonthlySaving(car.Id, "Car Fund", 5_000m, 25);
        await GivenMonthlySaving(car.Id, "Car Fund Extra", 1_000m, 10);
        await GivenMonthlySaving(holiday.Id, "Holiday Fund", 1_500m, 5);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(6_000m, response.Result!.Single(v => v.Id == car.Id).MonthlyContribution);
        Assert.Equal(1_500m, response.Result!.Single(v => v.Id == holiday.Id).MonthlyContribution);
    }

    // A goal with no monthly savings has no row in the grouped result at all, so the handler's
    // dictionary lookup has to fall back to zero.
    [Fact]
    public async Task Reports_Zero_Contribution_For_A_Goal_With_No_Monthly_Savings()
    {
        await GivenSavingGoal("New Car", 250_000m, Today.AddYears(1));

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        Assert.Equal(0m, Assert.Single(response.Result!).MonthlyContribution);
    }

    [Fact]
    public async Task Computes_Amount_Remaining_And_Days_Remaining()
    {
        await GivenSavingGoal("New Car", 250_000m, Today.AddDays(90), currentAmount: 80_000m);

        var response = await Handler.Handle(new GetSavingGoalsQuery(), CancellationToken.None);

        var vm = Assert.Single(response.Result!);
        Assert.Equal(80_000m, vm.AmountSaved);
        Assert.Equal(170_000m, vm.AmountRemaining);
        Assert.Equal(90, vm.DaysRemaining);
    }
}
