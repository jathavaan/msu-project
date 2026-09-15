using FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;

namespace FinanceOne.UnitTests.Features.SavingGoals.DeleteSavingGoal;

public class DeleteSavingGoalHandlerTests
{
    private readonly IDeleteSavingGoalRepository _repository = Substitute.For<IDeleteSavingGoalRepository>();

    private DeleteSavingGoalHandler Handler => new(_repository);

    private static SavingGoal AGoal(Guid id) => new()
    {
        Id = id,
        Name = "New Car",
        TargetAmount = 250_000m,
        CurrentAmount = 0m,
        TargetDate = new DateOnly(2027, 6, 15),
    };

    [Fact]
    public async Task Returns_404_When_The_Goal_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SavingGoal?)null);

        var response = await Handler.Handle(new DeleteSavingGoalCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // MonthlySaving -> SavingGoal is OnDelete(Restrict), so the guard turns what would be a
    // database-level failure into an expected 409.
    [Fact]
    public async Task Returns_409_When_A_Monthly_Saving_Still_References_It()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(AGoal(id));
        _repository.IsReferenced(id, Arg.Any<CancellationToken>()).Returns(true);

        var response = await Handler.Handle(new DeleteSavingGoalCommand(id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        await _repository.DidNotReceive().Delete(Arg.Any<SavingGoal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deletes_An_Unreferenced_Goal()
    {
        var id = Guid.NewGuid();
        var savingGoal = AGoal(id);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(savingGoal);
        _repository.IsReferenced(id, Arg.Any<CancellationToken>()).Returns(false);

        var response = await Handler.Handle(new DeleteSavingGoalCommand(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Delete(savingGoal, Arg.Any<CancellationToken>());
    }
}
