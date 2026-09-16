using FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;

namespace FinanceOne.UnitTests.Features.SavingGoals.CreateSavingGoal;

public class CreateSavingGoalHandlerTests
{
    private readonly ICreateSavingGoalRepository _repository = Substitute.For<ICreateSavingGoalRepository>();

    private CreateSavingGoalHandler Handler => new(_repository);

    [Fact]
    public async Task Creates_The_Saving_Goal_And_Returns_Its_Id()
    {
        var newId = Guid.NewGuid();
        var targetDate = new DateOnly(2027, 6, 15);
        _repository.Add(Arg.Any<SavingGoal>(), Arg.Any<CancellationToken>()).Returns(newId);

        var response = await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, targetDate), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(newId, response.Result);
        await _repository.Received(1).Add(
            Arg.Is<SavingGoal>(s => s.Name == "New Car" && s.TargetAmount == 250_000m && s.TargetDate == targetDate),
            Arg.Any<CancellationToken>());
    }

    // InterestRate is optional, so a command that omits it must persist InterestRate as null
    // rather than 0 — the projection treats those differently (see GetSavingGoalProjection).
    [Fact]
    public async Task Leaves_Interest_Rate_Null_When_Omitted()
    {
        _repository.Add(Arg.Any<SavingGoal>(), Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

        await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, new DateOnly(2027, 6, 15)), CancellationToken.None);

        await _repository.Received(1).Add(
            Arg.Is<SavingGoal>(s => s.InterestRate == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Persists_The_Interest_Rate_When_Given()
    {
        _repository.Add(Arg.Any<SavingGoal>(), Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

        await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, new DateOnly(2027, 6, 15), 4.5m), CancellationToken.None);

        await _repository.Received(1).Add(
            Arg.Is<SavingGoal>(s => s.InterestRate == 4.5m), Arg.Any<CancellationToken>());
    }

    // The create command carries no CurrentAmount — progress is tracked separately via
    // UpdateSavingGoal — so a new goal always starts at zero saved.
    [Fact]
    public async Task Starts_The_Goal_At_Zero_Saved()
    {
        _repository.Add(Arg.Any<SavingGoal>(), Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

        await Handler.Handle(
            new CreateSavingGoalCommand("New Car", 250_000m, new DateOnly(2027, 6, 15)), CancellationToken.None);

        await _repository.Received(1).Add(
            Arg.Is<SavingGoal>(s => s.CurrentAmount == 0m), Arg.Any<CancellationToken>());
    }
}
