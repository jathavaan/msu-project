using FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.MonthlySavings.CreateMonthlySaving;

public class CreateMonthlySavingTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private CreateMonthlySavingHandler Handler => new(new CreateMonthlySavingRepository(Context));

    [Fact]
    public async Task Persists_The_Monthly_Saving()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));

        var response = await Handler.Handle(
            new CreateMonthlySavingCommand("Car Fund", 5_000m, goal.Id, 25), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.MonthlySavings.SingleAsync(m => m.Id == response.Result);
        Assert.Equal("Car Fund", saved.Name);
        Assert.Equal(5_000m, saved.Amount);
        Assert.Equal(goal.Id, saved.SavingGoalId);
        Assert.Equal(25, saved.RecurrenceDay);
    }

    [Fact]
    public async Task Returns_404_When_The_Saving_Goal_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new CreateMonthlySavingCommand("Car Fund", 5_000m, Guid.NewGuid(), 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await using var context = NewContext();
        Assert.Empty(await context.MonthlySavings.ToListAsync());
    }

    [Fact]
    public async Task Rounds_The_Amount_To_Two_Decimal_Places()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));

        var response = await Handler.Handle(
            new CreateMonthlySavingCommand("Car Fund", 999.999m, goal.Id, 25), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(1_000.00m, (await context.MonthlySavings.SingleAsync(m => m.Id == response.Result)).Amount);
    }

    // One goal can be funded from several monthly savings — that is exactly what the contribution
    // total in GetSavingGoals sums up.
    [Fact]
    public async Task Allows_Several_Monthly_Savings_For_One_Goal()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));

        await Handler.Handle(new CreateMonthlySavingCommand("Salary Transfer", 5_000m, goal.Id, 25), CancellationToken.None);
        await Handler.Handle(new CreateMonthlySavingCommand("Bonus Transfer", 1_000m, goal.Id, 10), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(2, await context.MonthlySavings.CountAsync(m => m.SavingGoalId == goal.Id));
    }
}
