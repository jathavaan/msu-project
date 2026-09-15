using FinanceOne.Api.Features.Budgets.DeleteBudget;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Budgets.DeleteBudget;

public class DeleteBudgetTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private DeleteBudgetHandler Handler => new(new DeleteBudgetRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Budget_Does_Not_Exist()
    {
        var response = await Handler.Handle(new DeleteBudgetCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Removes_The_Budget_Row()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var budget = await GivenBudget(category.Id, 12_000m);

        var response = await Handler.Handle(new DeleteBudgetCommand(budget.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.False(await context.Budgets.AnyAsync(b => b.Id == budget.Id));
    }

    // Budget is the dependent side of the one-to-one, so deleting it must not take the category
    // with it — the category still classifies income/expense rows.
    [Fact]
    public async Task Leaves_The_Category_In_Place()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var budget = await GivenBudget(category.Id, 12_000m);

        await Handler.Handle(new DeleteBudgetCommand(budget.Id), CancellationToken.None);

        await using var context = NewContext();
        Assert.True(await context.Categories.AnyAsync(c => c.Id == category.Id));
    }

    // The unique index on CategoryId means a category can only ever hold one budget; deleting must
    // genuinely free that slot rather than leave a row behind.
    [Fact]
    public async Task Frees_The_Category_For_A_New_Budget()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var budget = await GivenBudget(category.Id, 12_000m);

        await Handler.Handle(new DeleteBudgetCommand(budget.Id), CancellationToken.None);
        await GivenBudget(category.Id, 9_000m);

        await using var context = NewContext();
        var replacement = await context.Budgets.SingleAsync(b => b.CategoryId == category.Id);
        Assert.Equal(9_000m, replacement.MonthlyLimit);
    }
}
