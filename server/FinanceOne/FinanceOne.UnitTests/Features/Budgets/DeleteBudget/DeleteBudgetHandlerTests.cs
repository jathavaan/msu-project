using FinanceOne.Api.Features.Budgets.DeleteBudget;

namespace FinanceOne.UnitTests.Features.Budgets.DeleteBudget;

public class DeleteBudgetHandlerTests
{
    private readonly IDeleteBudgetRepository _repository = Substitute.For<IDeleteBudgetRepository>();

    private DeleteBudgetHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_Budget_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Budget?)null);

        var response = await Handler.Handle(new DeleteBudgetCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Delete(Arg.Any<Budget>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deletes_An_Existing_Budget()
    {
        var id = Guid.NewGuid();
        var budget = new Budget { Id = id, CategoryId = Guid.NewGuid(), MonthlyLimit = 1_000m };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(budget);

        var response = await Handler.Handle(new DeleteBudgetCommand(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Delete(budget, Arg.Any<CancellationToken>());
    }
}
