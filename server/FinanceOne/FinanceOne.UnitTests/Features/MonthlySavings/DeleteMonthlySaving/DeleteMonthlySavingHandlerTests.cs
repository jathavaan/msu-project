using FinanceOne.Api.Features.MonthlySavings.DeleteMonthlySaving;

namespace FinanceOne.UnitTests.Features.MonthlySavings.DeleteMonthlySaving;

public class DeleteMonthlySavingHandlerTests
{
    private readonly IDeleteMonthlySavingRepository _repository = Substitute.For<IDeleteMonthlySavingRepository>();

    private DeleteMonthlySavingHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Monthly_Saving_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MonthlySaving?)null);

        var response = await Handler.Handle(new DeleteMonthlySavingCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Delete(Arg.Any<MonthlySaving>(), Arg.Any<CancellationToken>());
    }

    // Nothing references a MonthlySaving, so it is the dependent end and always deletable.
    [Fact]
    public async Task Deletes_An_Existing_Monthly_Saving()
    {
        var id = Guid.NewGuid();
        var monthlySaving = new MonthlySaving
        {
            Id = id,
            Name = "Car Fund",
            Amount = 5_000m,
            SavingGoalId = Guid.NewGuid(),
            RecurrenceDay = 25,
        };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(monthlySaving);

        var response = await Handler.Handle(new DeleteMonthlySavingCommand(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Delete(monthlySaving, Arg.Any<CancellationToken>());
    }
}
