using FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.CreateSavingGoal;

public class CreateSavingGoalTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private CreateSavingGoalHandler Handler => new(new CreateSavingGoalRepository(Context));

    [Fact]
    public async Task Persists_The_Saving_Goal()
    {
        var targetDate = new DateOnly(2027, 6, 15);

        var response = await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, targetDate), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.SavingGoals.SingleAsync(s => s.Id == response.Result);
        Assert.Equal("New Car", saved.Name);
        Assert.Equal(250_000m, saved.TargetAmount);
    }

    // TargetDate is a DateOnly mapped onto a MySQL `date` column through an explicit
    // DateTime conversion, because MySqlDataReader cannot read a `date` back into DateOnly
    // directly (see SavingGoalConfiguration). A round-trip is the only way to catch that
    // conversion regressing.
    [Fact]
    public async Task Round_Trips_The_Target_Date()
    {
        var targetDate = new DateOnly(2027, 6, 15);

        var response = await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, targetDate), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(targetDate, (await context.SavingGoals.SingleAsync(s => s.Id == response.Result)).TargetDate);
    }

    [Fact]
    public async Task Starts_The_Goal_At_Zero_Saved()
    {
        var response = await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, new DateOnly(2027, 6, 15)), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(0m, (await context.SavingGoals.SingleAsync(s => s.Id == response.Result)).CurrentAmount);
    }

    [Fact]
    public async Task Rounds_The_Target_Amount_To_Two_Decimal_Places()
    {
        var response = await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000.567m, new DateOnly(2027, 6, 15)), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(250_000.57m, (await context.SavingGoals.SingleAsync(s => s.Id == response.Result)).TargetAmount);
    }

    [Fact]
    public async Task Persists_A_Null_Interest_Rate_When_Omitted()
    {
        var response = await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, new DateOnly(2027, 6, 15)), CancellationToken.None);

        await using var context = NewContext();
        Assert.Null((await context.SavingGoals.SingleAsync(s => s.Id == response.Result)).InterestRate);
    }

    [Fact]
    public async Task Rounds_The_Interest_Rate_To_Two_Decimal_Places()
    {
        var response = await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, new DateOnly(2027, 6, 15), 4.567m), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(4.57m, (await context.SavingGoals.SingleAsync(s => s.Id == response.Result)).InterestRate);
    }

    // Nothing enforces uniqueness on a goal's name, so two goals can legitimately share one.
    [Fact]
    public async Task Allows_Two_Goals_With_The_Same_Name()
    {
        await Handler.Handle(
            new CreateSavingGoalCommand("Holiday", 30_000m, new DateOnly(2027, 6, 15)), CancellationToken.None);
        var response = await Handler.Handle(
            new CreateSavingGoalCommand("Holiday", 50_000m, new DateOnly(2028, 6, 15)), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.Equal(2, await context.SavingGoals.CountAsync(s => s.Name == "Holiday"));
    }
}
