using FinanceOne.Api.Features.Expenses.DeleteExpense;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Expenses.DeleteExpense;

public class DeleteExpenseTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private DeleteExpenseHandler Handler => new(new DeleteExpenseRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Expense_Does_Not_Exist()
    {
        var response = await Handler.Handle(new DeleteExpenseCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Removes_The_Expense_Row()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var expense = await GivenExpense(category.Id, "Monthly Rent", 12_000m, 1);

        var response = await Handler.Handle(new DeleteExpenseCommand(expense.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.False(await context.Expenses.AnyAsync(e => e.Id == expense.Id));
    }

    [Fact]
    public async Task Leaves_The_Category_In_Place()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var expense = await GivenExpense(category.Id, "Monthly Rent", 12_000m, 1);

        await Handler.Handle(new DeleteExpenseCommand(expense.Id), CancellationToken.None);

        await using var context = NewContext();
        Assert.True(await context.Categories.AnyAsync(c => c.Id == category.Id));
    }

    // Deleting one expense must not take its siblings with it — they share a category, not a row.
    [Fact]
    public async Task Leaves_Other_Expenses_In_The_Same_Category_Alone()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);
        var netflix = await GivenExpense(category.Id, "Netflix", 149m, 1);
        var spotify = await GivenExpense(category.Id, "Spotify", 119m, 1);

        await Handler.Handle(new DeleteExpenseCommand(netflix.Id), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(spotify.Id, (await context.Expenses.SingleAsync()).Id);
    }
}
