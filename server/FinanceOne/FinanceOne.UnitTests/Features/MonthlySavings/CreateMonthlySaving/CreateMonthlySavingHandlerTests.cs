using FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;

namespace FinanceOne.UnitTests.Features.MonthlySavings.CreateMonthlySaving;

public class CreateMonthlySavingHandlerTests
{
    private readonly ICreateMonthlySavingRepository _repository = Substitute.For<ICreateMonthlySavingRepository>();

    private CreateMonthlySavingHandler Handler => new(_repository);

    // A monthly saving only means something as a contribution towards a goal, so an orphaned one
    // is rejected before it can reach the FK.
    [Fact]
    public async Task Returns_404_When_The_Saving_Goal_Does_Not_Exist()
    {
        _repository.GetSavingGoal(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SavingGoal?)null);

        var response = await Handler.Handle(
            new CreateMonthlySavingCommand("Car Fund", 5_000m, Guid.NewGuid(), 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<MonthlySaving>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creates_The_Monthly_Saving_And_Returns_Its_Id()
    {
        var savingGoalId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        _repository.GetSavingGoal(savingGoalId, Arg.Any<CancellationToken>()).Returns(new SavingGoal
        {
            Id = savingGoalId,
            Name = "New Car",
            TargetAmount = 250_000m,
            TargetDate = new DateOnly(2027, 6, 15),
        });
        _repository.Add(Arg.Any<MonthlySaving>(), Arg.Any<CancellationToken>()).Returns(newId);

        var response = await Handler.Handle(
            new CreateMonthlySavingCommand("Car Fund", 5_000m, savingGoalId, 25), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(newId, response.Result);
        await _repository.Received(1).Add(
            Arg.Is<MonthlySaving>(m =>
                m.Name == "Car Fund"
                && m.Amount == 5_000m
                && m.SavingGoalId == savingGoalId
                && m.RecurrenceDay == 25),
            Arg.Any<CancellationToken>());
    }
}
