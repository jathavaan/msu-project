using FinanceOne.Api.Features.MonthlySavings.GetMonthlySavings;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.MonthlySavings.GetMonthlySavings;

public class GetMonthlySavingsTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetMonthlySavingsHandler Handler => new(new GetMonthlySavingsRepository(Context));

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_None()
    {
        var response = await Handler.Handle(new GetMonthlySavingsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Includes_The_Saving_Goal_Name_From_The_Joined_Goal()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        var response = await Handler.Handle(new GetMonthlySavingsQuery(), CancellationToken.None);

        var monthlySaving = Assert.Single(response.Result!);
        Assert.Equal("Car Fund", monthlySaving.Name);
        Assert.Equal("New Car", monthlySaving.SavingGoalName);
        Assert.Equal(5_000m, monthlySaving.Amount);
        Assert.Equal(25, monthlySaving.RecurrenceDay);
    }

    [Fact]
    public async Task Orders_By_Name()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        await GivenMonthlySaving(goal.Id, "Salary Transfer", 5_000m, 25);
        await GivenMonthlySaving(goal.Id, "Bonus Transfer", 1_000m, 10);
        await GivenMonthlySaving(goal.Id, "Interest Transfer", 200m, 1);

        var response = await Handler.Handle(new GetMonthlySavingsQuery(), CancellationToken.None);

        Assert.Equal(
            ["Bonus Transfer", "Interest Transfer", "Salary Transfer"],
            response.Result!.Select(m => m.Name));
    }

    // The list is global rather than per-goal, so contributions to different goals appear together.
    [Fact]
    public async Task Returns_Monthly_Savings_Across_Every_Goal()
    {
        var car = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var holiday = await GivenSavingGoal("Holiday", 30_000m, new DateOnly(2027, 8, 1));
        await GivenMonthlySaving(car.Id, "Car Fund", 5_000m, 25);
        await GivenMonthlySaving(holiday.Id, "Holiday Fund", 1_500m, 5);

        var response = await Handler.Handle(new GetMonthlySavingsQuery(), CancellationToken.None);

        Assert.Equal(2, response.Result!.Count);
        Assert.Contains(response.Result!, m => m.SavingGoalName == "New Car");
        Assert.Contains(response.Result!, m => m.SavingGoalName == "Holiday");
    }
}
