using FinanceOne.Api.Features.Categories.DeleteCategory;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Categories.DeleteCategory;

public class DeleteCategoryTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private DeleteCategoryHandler Handler => new(new DeleteCategoryRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var response = await Handler.Handle(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Removes_An_Unreferenced_Category()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.False(await context.Categories.AnyAsync(c => c.Id == category.Id));
    }

    // Every FK onto Category is OnDelete(Restrict), so without the IsReferenced guard MySQL would
    // reject the DELETE and the slice would surface a 500 instead of a 409.
    [Fact]
    public async Task Returns_409_When_An_Expense_Still_References_It()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        await GivenExpense(category.Id, "Monthly Rent", 12_000m, recurrenceDay: 1);

        var response = await Handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);

        await using var context = NewContext();
        Assert.True(await context.Categories.AnyAsync(c => c.Id == category.Id));
    }

    [Fact]
    public async Task Returns_409_When_A_Budget_Still_References_It()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        await GivenBudget(category.Id, 12_000m);

        var response = await Handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_409_When_An_Income_Still_References_It()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);
        await GivenIncome(category.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
    }

    // Once the last reference is gone the category becomes deletable again — the guard is about
    // live references, not about the category having ever been used.
    [Fact]
    public async Task Becomes_Deletable_Once_The_Reference_Is_Removed()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var expense = await GivenExpense(category.Id, "Monthly Rent", 12_000m, recurrenceDay: 1);

        Context.Expenses.Remove(expense);
        await Context.SaveChangesAsync();

        var response = await Handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
