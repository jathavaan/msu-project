using FinanceOne.Api.Features.CategorizationRules.DeleteCategorizationRule;

namespace FinanceOne.UnitTests.Features.CategorizationRules.DeleteCategorizationRule;

public class DeleteCategorizationRuleHandlerTests
{
    private readonly IDeleteCategorizationRuleRepository _repository = Substitute.For<IDeleteCategorizationRuleRepository>();

    private DeleteCategorizationRuleHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Rule_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CategorizationRule?)null);

        var response = await Handler.Handle(new DeleteCategorizationRuleCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Delete(Arg.Any<CategorizationRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deletes_The_Rule()
    {
        var rule = new CategorizationRule { Id = Guid.NewGuid(), Keyword = "REMA", CategoryId = Guid.NewGuid() };
        _repository.GetById(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var response = await Handler.Handle(new DeleteCategorizationRuleCommand(rule.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Delete(rule, Arg.Any<CancellationToken>());
    }
}
