using FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.GetSavingGoalById;

public class GetSavingGoalByIdTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetSavingGoalByIdHandler Handler => new(new GetSavingGoalByIdRepository(Context), _timeProvider);

    [Fact]
    public async Task Returns_404_When_The_Goal_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetSavingGoalByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Goal_With_Its_Derived_Fields()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, Today.AddDays(90), currentAmount: 80_000m);
        await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        var response = await Handler.Handle(new GetSavingGoalByIdQuery(goal.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var vm = response.Result!;
        Assert.Equal("New Car", vm.Name);
        Assert.Equal(Today.AddDays(90), vm.TargetDate);
        Assert.Equal(170_000m, vm.AmountRemaining);
        Assert.Equal(90, vm.DaysRemaining);
        Assert.Equal(5_000m, vm.MonthlyContribution);
    }

    // The total comes from SumAsync over a nullable projection, which returns null (not zero) when
    // there are no rows — the repository coalesces that, and this is what proves it.
    [Fact]
    public async Task Reports_Zero_Contribution_When_Nothing_Funds_The_Goal()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, Today.AddDays(90));

        var response = await Handler.Handle(new GetSavingGoalByIdQuery(goal.Id), CancellationToken.None);

        Assert.Equal(0m, response.Result!.MonthlyContribution);
    }

    // Only this goal's own monthly savings count towards it.
    [Fact]
    public async Task Ignores_Monthly_Savings_Belonging_To_Another_Goal()
    {
        var car = await GivenSavingGoal("New Car", 250_000m, Today.AddDays(90));
        var holiday = await GivenSavingGoal("Holiday", 30_000m, Today.AddDays(120));
        await GivenMonthlySaving(car.Id, "Car Fund", 5_000m, 25);
        await GivenMonthlySaving(holiday.Id, "Holiday Fund", 1_500m, 5);

        var response = await Handler.Handle(new GetSavingGoalByIdQuery(car.Id), CancellationToken.None);

        Assert.Equal(5_000m, response.Result!.MonthlyContribution);
    }
}
