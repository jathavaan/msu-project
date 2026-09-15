using FinanceOne.Api.Features.MonthlySavings.UpdateMonthlySaving;

namespace FinanceOne.UnitTests.Features.MonthlySavings.UpdateMonthlySaving;

public class UpdateMonthlySavingHandlerTests
{
    private readonly IUpdateMonthlySavingRepository _repository = Substitute.For<IUpdateMonthlySavingRepository>();

    private UpdateMonthlySavingHandler Handler => new(_repository);

    private static MonthlySaving AMonthlySaving(Guid id, Guid savingGoalId) => new()
    {
        Id = id,
        Name = "Car Fund",
        Amount = 5_000m,
        SavingGoalId = savingGoalId,
        RecurrenceDay = 25,
    };

    private static SavingGoal AGoal(Guid id) => new()
    {
        Id = id,
        Name = "New Car",
        TargetAmount = 250_000m,
        TargetDate = new DateOnly(2027, 6, 15),
    };

    [Fact]
    public async Task Returns_404_When_The_Monthly_Saving_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MonthlySaving?)null);

        var response = await Handler.Handle(
            new UpdateMonthlySavingCommand(Guid.NewGuid(), "Car Fund", 5_000m, Guid.NewGuid(), 25),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    // An update can re-point the contribution at a different goal, so the target goal is re-checked
    // every time rather than only at creation.
    [Fact]
    public async Task Returns_404_When_The_Target_Saving_Goal_Does_Not_Exist()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(AMonthlySaving(id, Guid.NewGuid()));
        _repository.GetSavingGoal(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SavingGoal?)null);

        var response = await Handler.Handle(
            new UpdateMonthlySavingCommand(id, "Car Fund", 5_000m, Guid.NewGuid(), 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Applies_Every_Field_Then_Saves()
    {
        var id = Guid.NewGuid();
        var newGoalId = Guid.NewGuid();
        var monthlySaving = AMonthlySaving(id, Guid.NewGuid());
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(monthlySaving);
        _repository.GetSavingGoal(newGoalId, Arg.Any<CancellationToken>()).Returns(AGoal(newGoalId));

        var response = await Handler.Handle(
            new UpdateMonthlySavingCommand(id, "Holiday Fund", 1_500m, newGoalId, 5), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("Holiday Fund", monthlySaving.Name);
        Assert.Equal(1_500m, monthlySaving.Amount);
        Assert.Equal(newGoalId, monthlySaving.SavingGoalId);
        Assert.Equal(5, monthlySaving.RecurrenceDay);
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }
}
