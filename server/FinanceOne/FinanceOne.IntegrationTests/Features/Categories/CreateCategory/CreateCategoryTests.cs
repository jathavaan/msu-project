using FinanceOne.Api.Features.Categories.CreateCategory;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Categories.CreateCategory;

public class CreateCategoryTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private CreateCategoryHandler Handler => new(new CreateCategoryRepository(Context));

    [Fact]
    public async Task Persists_The_Category()
    {
        var response = await Handler.Handle(new CreateCategoryCommand("Rent", CategoryType.Expense), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Categories.SingleAsync(c => c.Id == response.Result);
        Assert.Equal("Rent", saved.Name);
        Assert.Equal(CategoryType.Expense, saved.Type);
    }

    [Fact]
    public async Task Returns_409_For_A_Duplicate_Name_And_Type()
    {
        await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(new CreateCategoryCommand("Rent", CategoryType.Expense), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);

        await using var context = NewContext();
        Assert.Single(await context.Categories.Where(c => c.Name == "Rent").ToListAsync());
    }

    // Uniqueness is on the (name, type) pair rather than the name alone, so the same label can
    // exist once on each side of the ledger.
    [Fact]
    public async Task Allows_The_Same_Name_Under_The_Other_Type()
    {
        await GivenCategory("Gifts", CategoryType.Expense);

        var response = await Handler.Handle(new CreateCategoryCommand("Gifts", CategoryType.Income), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.Equal(2, await context.Categories.CountAsync(c => c.Name == "Gifts"));
    }

    // MySQL's default collation is case-insensitive, so the AnyAsync duplicate check treats "rent"
    // and "Rent" as the same name. Pinning this down because it is collation-driven, not code-driven
    // — the same query on a case-sensitive engine would let both rows through.
    [Fact]
    public async Task Treats_Names_Differing_Only_In_Case_As_Duplicates()
    {
        await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(new CreateCategoryCommand("rent", CategoryType.Expense), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
    }
}
