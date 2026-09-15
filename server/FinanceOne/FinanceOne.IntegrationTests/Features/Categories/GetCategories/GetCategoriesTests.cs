using FinanceOne.Api.Features.Categories.GetCategories;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Categories.GetCategories;

public class GetCategoriesTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetCategoriesHandler Handler => new(new GetCategoriesRepository(Context));

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_No_Categories()
    {
        var response = await Handler.Handle(new GetCategoriesQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Returns_Every_Category_When_No_Type_Filter_Is_Given()
    {
        await GivenCategory("Salary", CategoryType.Income);
        await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(new GetCategoriesQuery(null), CancellationToken.None);

        Assert.Equal(2, response.Result!.Count);
    }

    [Theory]
    [InlineData(CategoryType.Income)]
    [InlineData(CategoryType.Expense)]
    public async Task Filters_By_Type(CategoryType type)
    {
        await GivenCategory("Salary", CategoryType.Income);
        await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(new GetCategoriesQuery(type), CancellationToken.None);

        var category = Assert.Single(response.Result!);
        Assert.Equal(type, category.Type);
    }

    // The dropdowns on every form render this list as-is, so the ordering is part of the contract
    // rather than an implementation detail of the query.
    [Fact]
    public async Task Orders_By_Name()
    {
        await GivenCategory("Utilities", CategoryType.Expense);
        await GivenCategory("Food & Drinks", CategoryType.Expense);
        await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(new GetCategoriesQuery(null), CancellationToken.None);

        Assert.Equal(["Food & Drinks", "Rent", "Utilities"], response.Result!.Select(c => c.Name));
    }
}
