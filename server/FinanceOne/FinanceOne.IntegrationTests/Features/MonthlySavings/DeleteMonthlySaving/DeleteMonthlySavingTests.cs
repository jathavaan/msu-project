using FinanceOne.Api.Features.MonthlySavings.DeleteMonthlySaving;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.MonthlySavings.DeleteMonthlySaving;

public class DeleteMonthlySavingTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private DeleteMonthlySavingHandler Handler => new(new DeleteMonthlySavingRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Monthly_Saving_Does_Not_Exist()
    {
        var response = await Handler.Handle(new DeleteMonthlySavingCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Removes_The_Monthly_Saving_Row()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var monthlySaving = await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        var response = await Handler.Handle(
            new DeleteMonthlySavingCommand(monthlySaving.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.False(await context.MonthlySavings.AnyAsync(m => m.Id == monthlySaving.Id));
    }

    // MonthlySaving is the dependent side, so deleting it must not cascade into the goal it funds.
    [Fact]
    public async Task Leaves_The_Saving_Goal_In_Place()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var monthlySaving = await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        await Handler.Handle(new DeleteMonthlySavingCommand(monthlySaving.Id), CancellationToken.None);

        await using var context = NewContext();
        Assert.True(await context.SavingGoals.AnyAsync(s => s.Id == goal.Id));
    }

    // Removing the last one is what unblocks DeleteSavingGoal's 409.
    [Fact]
    public async Task Frees_The_Goal_For_Deletion()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var monthlySaving = await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        await Handler.Handle(new DeleteMonthlySavingCommand(monthlySaving.Id), CancellationToken.None);

        await using var context = NewContext();
        Assert.False(await context.MonthlySavings.AnyAsync(m => m.SavingGoalId == goal.Id));
    }
}
