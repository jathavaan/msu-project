using FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.CategorizationRules.CreateCategorizationRule;

public class CreateCategorizationRuleTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private CreateCategorizationRuleHandler Handler => new(new CreateCategorizationRuleRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new CreateCategorizationRuleCommand("REMA", Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Persists_The_Rule()
    {
        var category = await GivenCategory("Food", CategoryType.Expense);

        var response = await Handler.Handle(
            new CreateCategorizationRuleCommand("REMA", category.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.CategorizationRules.SingleAsync(r => r.Id == response.Result);
        Assert.Equal("REMA", saved.Keyword);
        Assert.Equal(category.Id, saved.CategoryId);
    }
}
