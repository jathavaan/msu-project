using FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.UpdateSavingGoal;

public class UpdateSavingGoalTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateSavingGoalHandler Handler => new(new UpdateSavingGoalRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Goal_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new UpdateSavingGoalCommand(Guid.NewGuid(), "New Car", 250_000m, new DateOnly(2027, 6, 15), 0m),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Persists_Every_Changed_Field()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var newTargetDate = new DateOnly(2028, 1, 1);

        var response = await Handler.Handle(
            new UpdateSavingGoalCommand(goal.Id, "Used Car", 180_000m, newTargetDate, 45_000m),
            CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.SavingGoals.SingleAsync(s => s.Id == goal.Id);
        Assert.Equal("Used Car", saved.Name);
        Assert.Equal(180_000m, saved.TargetAmount);
        Assert.Equal(newTargetDate, saved.TargetDate);
        Assert.Equal(45_000m, saved.CurrentAmount);
    }

    // Recording progress is the most common edit, and it is the only mechanism for it — there is
    // no separate contribution slice.
    [Fact]
    public async Task Records_Progress_Towards_The_Goal()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15), currentAmount: 45_000m);

        await Handler.Handle(
            new UpdateSavingGoalCommand(goal.Id, "New Car", 250_000m, new DateOnly(2027, 6, 15), 60_000m),
            CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(60_000m, (await context.SavingGoals.SingleAsync(s => s.Id == goal.Id)).CurrentAmount);
    }

    // Editing a goal must not disturb what funds it.
    [Fact]
    public async Task Leaves_Its_Monthly_Savings_Untouched()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        await Handler.Handle(
            new UpdateSavingGoalCommand(goal.Id, "Used Car", 180_000m, new DateOnly(2027, 6, 15), 0m),
            CancellationToken.None);

        await using var context = NewContext();
        Assert.Single(await context.MonthlySavings.Where(m => m.SavingGoalId == goal.Id).ToListAsync());
    }
}
