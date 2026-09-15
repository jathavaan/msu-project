using FinanceOne.Api.Features.Budgets.UpdateBudget;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Budgets.UpdateBudget;

public class UpdateBudgetTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateBudgetHandler Handler => new(new UpdateBudgetRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Budget_Does_Not_Exist()
    {
        var response = await Handler.Handle(new UpdateBudgetCommand(Guid.NewGuid(), 5_000m), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Persists_The_New_Monthly_Limit()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var budget = await GivenBudget(category.Id, 12_000m);

        var response = await Handler.Handle(new UpdateBudgetCommand(budget.Id, 13_500m), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Budgets.SingleAsync(b => b.Id == budget.Id);
        Assert.Equal(13_500m, saved.MonthlyLimit);
    }

    // The command carries no CategoryId — a budget's category is fixed at creation — so an update
    // must leave the link alone rather than clearing or re-pointing it.
    [Fact]
    public async Task Leaves_The_Category_Link_Untouched()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var budget = await GivenBudget(category.Id, 12_000m);

        await Handler.Handle(new UpdateBudgetCommand(budget.Id, 13_500m), CancellationToken.None);

        await using var context = NewContext();
        var saved = await context.Budgets.SingleAsync(b => b.Id == budget.Id);
        Assert.Equal(category.Id, saved.CategoryId);
    }
}
