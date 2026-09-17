using FinanceOne.Api.Features.CategorizationRules.DeleteCategorizationRule;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.CategorizationRules.DeleteCategorizationRule;

public class DeleteCategorizationRuleTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private DeleteCategorizationRuleHandler Handler => new(new DeleteCategorizationRuleRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Rule_Does_Not_Exist()
    {
        var response = await Handler.Handle(new DeleteCategorizationRuleCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Deletes_The_Rule()
    {
        var category = await GivenCategory("Food", CategoryType.Expense);
        var rule = await GivenCategorizationRule("REMA", category.Id);

        var response = await Handler.Handle(new DeleteCategorizationRuleCommand(rule.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.False(await context.CategorizationRules.AnyAsync(r => r.Id == rule.Id));
    }
}
