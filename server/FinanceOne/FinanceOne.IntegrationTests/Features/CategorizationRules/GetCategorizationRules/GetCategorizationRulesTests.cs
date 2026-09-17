using FinanceOne.Api.Features.CategorizationRules.GetCategorizationRules;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.CategorizationRules.GetCategorizationRules;

public class GetCategorizationRulesTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetCategorizationRulesHandler Handler => new(new GetCategorizationRulesRepository(Context));

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_No_Rules()
    {
        var response = await Handler.Handle(new GetCategorizationRulesQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Returns_Every_Rule_Ordered_By_Keyword_With_Its_Category_Name()
    {
        var food = await GivenCategory("Food", CategoryType.Expense);
        await GivenCategorizationRule("REMA", food.Id);
        await GivenCategorizationRule("Kiwi", food.Id);

        var response = await Handler.Handle(new GetCategorizationRulesQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(["Kiwi", "REMA"], response.Result!.Select(r => r.Keyword));
        Assert.All(response.Result!, r => Assert.Equal("Food", r.CategoryName));
    }
}
