using FinanceOne.Api.Features.Expenses.DeleteExpense;

namespace FinanceOne.UnitTests.Features.Expenses.DeleteExpense;

public class DeleteExpenseHandlerTests
{
    private readonly IDeleteExpenseRepository _repository = Substitute.For<IDeleteExpenseRepository>();

    private DeleteExpenseHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Expense_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Expense?)null);

        var response = await Handler.Handle(new DeleteExpenseCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Delete(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
    }

    // Nothing references an Expense, so unlike DeleteCategory there is no conflict branch here.
    [Fact]
    public async Task Deletes_An_Existing_Expense()
    {
        var id = Guid.NewGuid();
        var expense = new Expense
        {
            Id = id,
            Name = "Rent",
            Amount = 12_000m,
            CategoryId = Guid.NewGuid(),
            RecurrenceDay = 1,
        };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(expense);

        var response = await Handler.Handle(new DeleteExpenseCommand(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Delete(expense, Arg.Any<CancellationToken>());
    }
}
