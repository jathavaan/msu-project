using FinanceOne.Api.Features.Income.DeleteIncome;

namespace FinanceOne.UnitTests.Features.Income.DeleteIncome;

public class DeleteIncomeHandlerTests
{
    private readonly IDeleteIncomeRepository _repository = Substitute.For<IDeleteIncomeRepository>();

    private DeleteIncomeHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Income_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IncomeEntity?)null);

        var response = await Handler.Handle(new DeleteIncomeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Delete(Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deletes_An_Existing_Income()
    {
        var id = Guid.NewGuid();
        var income = new IncomeEntity
        {
            Id = id,
            Name = "Monthly Salary",
            Amount = 45_000m,
            CategoryId = Guid.NewGuid(),
            RecurrenceDay = 25,
        };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(income);

        var response = await Handler.Handle(new DeleteIncomeCommand(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Delete(income, Arg.Any<CancellationToken>());
    }
}
