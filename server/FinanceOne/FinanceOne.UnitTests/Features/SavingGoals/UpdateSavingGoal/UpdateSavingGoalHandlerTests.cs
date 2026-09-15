using FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;

namespace FinanceOne.UnitTests.Features.SavingGoals.UpdateSavingGoal;

public class UpdateSavingGoalHandlerTests
{
    private readonly IUpdateSavingGoalRepository _repository = Substitute.For<IUpdateSavingGoalRepository>();

    private UpdateSavingGoalHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Goal_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SavingGoal?)null);

        var response = await Handler.Handle(
            new UpdateSavingGoalCommand(Guid.NewGuid(), "New Car", 250_000m, new DateOnly(2027, 6, 15), 0m),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    // This is also the only way progress is recorded — there is no separate "contribute" slice —
    // so CurrentAmount being applied is the point of the whole update.
    [Fact]
    public async Task Applies_Every_Field_Including_Current_Amount_Then_Saves()
    {
        var id = Guid.NewGuid();
        var newTargetDate = new DateOnly(2028, 1, 1);
        var savingGoal = new SavingGoal
        {
            Id = id,
            Name = "New Car",
            TargetAmount = 250_000m,
            CurrentAmount = 80_000m,
            TargetDate = new DateOnly(2027, 6, 15),
        };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(savingGoal);

        var response = await Handler.Handle(
            new UpdateSavingGoalCommand(id, "Used Car", 180_000m, newTargetDate, 95_000m), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("Used Car", savingGoal.Name);
        Assert.Equal(180_000m, savingGoal.TargetAmount);
        Assert.Equal(newTargetDate, savingGoal.TargetDate);
        Assert.Equal(95_000m, savingGoal.CurrentAmount);
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }
}
