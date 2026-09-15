using FinanceOne.Api.Features.Budgets.UpdateBudget;

namespace FinanceOne.UnitTests.Features.Budgets.UpdateBudget;

public class UpdateBudgetHandlerTests
{
    private readonly IUpdateBudgetRepository _repository = Substitute.For<IUpdateBudgetRepository>();

    private UpdateBudgetHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_Budget_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Budget?)null);

        var response = await Handler.Handle(new UpdateBudgetCommand(Guid.NewGuid(), 2_000m), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    // The handler mutates the tracked entity and lets the repository call SaveChanges — asserting
    // on the entity is what proves the new limit would actually be persisted.
    [Fact]
    public async Task Applies_The_New_Monthly_Limit_And_Saves()
    {
        var id = Guid.NewGuid();
        var budget = new Budget { Id = id, CategoryId = Guid.NewGuid(), MonthlyLimit = 1_000m };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(budget);

        var response = await Handler.Handle(new UpdateBudgetCommand(id, 2_500m), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(2_500m, budget.MonthlyLimit);
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }
}
