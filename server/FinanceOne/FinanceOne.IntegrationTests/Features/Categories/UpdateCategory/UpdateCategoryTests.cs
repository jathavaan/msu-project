using FinanceOne.Api.Features.Categories.UpdateCategory;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Categories.UpdateCategory;

public class UpdateCategoryTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateCategoryHandler Handler => new(new UpdateCategoryRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new UpdateCategoryCommand(Guid.NewGuid(), "Rent", CategoryType.Expense), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Persists_The_New_Name_And_Type()
    {
        var category = await GivenCategory("Groceries", CategoryType.Expense);

        var response = await Handler.Handle(
            new UpdateCategoryCommand(category.Id, "Food & Drinks", CategoryType.Expense), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Categories.SingleAsync(c => c.Id == category.Id);
        Assert.Equal("Food & Drinks", saved.Name);
    }

    [Fact]
    public async Task Returns_409_When_Another_Category_Already_Has_That_Name_And_Type()
    {
        await GivenCategory("Rent", CategoryType.Expense);
        var category = await GivenCategory("Groceries", CategoryType.Expense);

        var response = await Handler.Handle(
            new UpdateCategoryCommand(category.Id, "Rent", CategoryType.Expense), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);

        await using var context = NewContext();
        Assert.Equal("Groceries", (await context.Categories.SingleAsync(c => c.Id == category.Id)).Name);
    }

    // The duplicate check excludes the row being edited. Saving a category with its own name
    // unchanged is the most common edit there is, and it must not 409 against itself.
    [Fact]
    public async Task Saving_A_Category_Under_Its_Own_Name_Is_Not_A_Conflict()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(
            new UpdateCategoryCommand(category.Id, "Rent", CategoryType.Expense), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    // Flipping the type is allowed even while the name is taken on the other side, because the
    // uniqueness rule is per (name, type) pair.
    [Fact]
    public async Task Can_Switch_Type_When_The_Name_Exists_Under_The_Other_Type()
    {
        await GivenCategory("Gifts", CategoryType.Expense);
        var category = await GivenCategory("Bonus", CategoryType.Income);

        var response = await Handler.Handle(
            new UpdateCategoryCommand(category.Id, "Gifts", CategoryType.Income), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.Equal(2, await context.Categories.CountAsync(c => c.Name == "Gifts"));
    }
}
