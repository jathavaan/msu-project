using FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.SavingGoals.GetSavingGoalById;

public class GetSavingGoalByIdHandlerTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly IGetSavingGoalByIdRepository _repository = Substitute.For<IGetSavingGoalByIdRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetSavingGoalByIdHandler Handler => new(_repository, _timeProvider);

    [Fact]
    public async Task Returns_404_When_The_Goal_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SavingGoal?)null);

        var response = await Handler.Handle(new GetSavingGoalByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Projects_The_Goal_With_Its_Derived_Fields()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(new SavingGoal
        {
            Id = id,
            Name = "New Car",
            TargetAmount = 250_000m,
            CurrentAmount = 80_000m,
            TargetDate = Today.AddDays(45),
            InterestRate = 4.5m,
            ImageUrl = "/api/saving-goals/g1/image",
        });
        _repository.GetMonthlyContributionTotal(id, Arg.Any<CancellationToken>()).Returns(5_000m);

        var response = await Handler.Handle(new GetSavingGoalByIdQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var vm = response.Result!;
        Assert.Equal("New Car", vm.Name);
        Assert.Equal(250_000m, vm.TargetAmount);
        Assert.Equal(80_000m, vm.AmountSaved);
        Assert.Equal(170_000m, vm.AmountRemaining);
        Assert.Equal(45, vm.DaysRemaining);
        Assert.Equal(5_000m, vm.MonthlyContribution);
        Assert.Equal(4.5m, vm.InterestRate);
        Assert.Equal("/api/saving-goals/g1/image", vm.ImageUrl);
    }

    // Matches GetSavingGoals: an overdue goal reads zero days left rather than a negative number.
    [Fact]
    public async Task Clamps_Days_Remaining_At_Zero_For_An_Overdue_Goal()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(new SavingGoal
        {
            Id = id,
            Name = "Holiday",
            TargetAmount = 30_000m,
            CurrentAmount = 30_000m,
            TargetDate = Today.AddDays(-30),
        });

        var response = await Handler.Handle(new GetSavingGoalByIdQuery(id), CancellationToken.None);

        Assert.Equal(0, response.Result!.DaysRemaining);
    }
}
