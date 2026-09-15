using FinanceOne.Api.Features.MonthlySavings.UpdateMonthlySaving;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.MonthlySavings.UpdateMonthlySaving;

public class UpdateMonthlySavingTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateMonthlySavingHandler Handler => new(new UpdateMonthlySavingRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Monthly_Saving_Does_Not_Exist()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));

        var response = await Handler.Handle(
            new UpdateMonthlySavingCommand(Guid.NewGuid(), "Car Fund", 5_000m, goal.Id, 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_When_The_Target_Goal_Does_Not_Exist()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var monthlySaving = await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        var response = await Handler.Handle(
            new UpdateMonthlySavingCommand(monthlySaving.Id, "Car Fund", 5_000m, Guid.NewGuid(), 25),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);

        await using var context = NewContext();
        Assert.Equal(goal.Id, (await context.MonthlySavings.SingleAsync(m => m.Id == monthlySaving.Id)).SavingGoalId);
    }

    [Fact]
    public async Task Persists_Every_Changed_Field()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var monthlySaving = await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        var response = await Handler.Handle(
            new UpdateMonthlySavingCommand(monthlySaving.Id, "Car Fund (increased)", 6_500m, goal.Id, 10),
            CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.MonthlySavings.SingleAsync(m => m.Id == monthlySaving.Id);
        Assert.Equal("Car Fund (increased)", saved.Name);
        Assert.Equal(6_500m, saved.Amount);
        Assert.Equal(10, saved.RecurrenceDay);
    }

    // Re-pointing a contribution moves it between the two goals' contribution totals, which is the
    // figure GetSavingGoals reports.
    [Fact]
    public async Task Moves_The_Contribution_To_Another_Goal()
    {
        var car = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var holiday = await GivenSavingGoal("Holiday", 30_000m, new DateOnly(2027, 8, 1));
        var monthlySaving = await GivenMonthlySaving(car.Id, "Monthly Transfer", 5_000m, 25);

        await Handler.Handle(
            new UpdateMonthlySavingCommand(monthlySaving.Id, "Monthly Transfer", 5_000m, holiday.Id, 25),
            CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(holiday.Id, (await context.MonthlySavings.SingleAsync(m => m.Id == monthlySaving.Id)).SavingGoalId);
        Assert.False(await context.MonthlySavings.AnyAsync(m => m.SavingGoalId == car.Id));
    }
}
