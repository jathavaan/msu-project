using FinanceOne.Api.Features.Expenses.CreateExpense;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Expenses.CreateExpense;

public class CreateExpenseTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private CreateExpenseHandler Handler => new(new CreateExpenseRepository(Context));

    [Fact]
    public async Task Persists_The_Expense()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(
            new CreateExpenseCommand("Monthly Rent", 12_000m, category.Id, 1), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Expenses.SingleAsync(e => e.Id == response.Result);
        Assert.Equal("Monthly Rent", saved.Name);
        Assert.Equal(12_000m, saved.Amount);
        Assert.Equal(category.Id, saved.CategoryId);
        Assert.Equal(1, saved.RecurrenceDay);
    }

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new CreateExpenseCommand("Monthly Rent", 12_000m, Guid.NewGuid(), 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await using var context = NewContext();
        Assert.Empty(await context.Expenses.ToListAsync());
    }

    [Fact]
    public async Task Returns_404_When_The_Category_Is_An_Income_Category()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);

        var response = await Handler.Handle(
            new CreateExpenseCommand("Monthly Rent", 12_000m, category.Id, 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // Amount is decimal(18,2) in MySQL, so anything finer is rounded on write.
    [Fact]
    public async Task Rounds_The_Amount_To_Two_Decimal_Places()
    {
        var category = await GivenCategory("Food & Drinks", CategoryType.Expense);

        var response = await Handler.Handle(
            new CreateExpenseCommand("Grocery Shopping", 249.999m, category.Id, 10), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(250.00m, (await context.Expenses.SingleAsync(e => e.Id == response.Result)).Amount);
    }

    // Unlike a budget there is no uniqueness rule, so a category can carry many expenses.
    [Fact]
    public async Task Allows_Several_Expenses_In_The_Same_Category()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);

        await Handler.Handle(new CreateExpenseCommand("Netflix", 149m, category.Id, 1), CancellationToken.None);
        await Handler.Handle(new CreateExpenseCommand("Spotify", 119m, category.Id, 1), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(2, await context.Expenses.CountAsync(e => e.CategoryId == category.Id));
    }
}
