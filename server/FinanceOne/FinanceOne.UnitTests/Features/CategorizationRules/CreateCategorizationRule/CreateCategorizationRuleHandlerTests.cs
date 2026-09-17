using FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;

namespace FinanceOne.UnitTests.Features.CategorizationRules.CreateCategorizationRule;

public class CreateCategorizationRuleHandlerTests
{
    private readonly ICreateCategorizationRuleRepository _repository = Substitute.For<ICreateCategorizationRuleRepository>();

    private CreateCategorizationRuleHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        _repository.GetCategory(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var response = await Handler.Handle(
            new CreateCategorizationRuleCommand("REMA", Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<CategorizationRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creates_The_Rule_And_Returns_Its_Id()
    {
        var categoryId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        _repository.GetCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Food", Type = CategoryType.Expense });
        _repository.Add(Arg.Any<CategorizationRule>(), Arg.Any<CancellationToken>()).Returns(newId);

        var response = await Handler.Handle(
            new CreateCategorizationRuleCommand("REMA", categoryId), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(newId, response.Result);
        await _repository.Received(1).Add(
            Arg.Is<CategorizationRule>(r => r.Keyword == "REMA" && r.CategoryId == categoryId),
            Arg.Any<CancellationToken>());
    }
}
