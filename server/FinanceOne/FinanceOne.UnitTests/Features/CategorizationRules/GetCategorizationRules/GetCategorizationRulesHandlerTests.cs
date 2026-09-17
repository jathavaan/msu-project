using FinanceOne.Api.Features.CategorizationRules.GetCategorizationRules;

namespace FinanceOne.UnitTests.Features.CategorizationRules.GetCategorizationRules;

public class GetCategorizationRulesHandlerTests
{
    private readonly IGetCategorizationRulesRepository _repository = Substitute.For<IGetCategorizationRulesRepository>();

    private GetCategorizationRulesHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_What_The_Repository_Produced()
    {
        var rules = new List<CategorizationRuleVm> { new(Guid.NewGuid(), "REMA", Guid.NewGuid(), "Food") };
        _repository.GetCategorizationRules(Arg.Any<CancellationToken>()).Returns(rules);

        var response = await Handler.Handle(new GetCategorizationRulesQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(rules, response.Result);
    }

    [Fact]
    public async Task Returns_Success_With_An_Empty_List_When_There_Are_No_Rules()
    {
        _repository.GetCategorizationRules(Arg.Any<CancellationToken>()).Returns([]);

        var response = await Handler.Handle(new GetCategorizationRulesQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }
}
