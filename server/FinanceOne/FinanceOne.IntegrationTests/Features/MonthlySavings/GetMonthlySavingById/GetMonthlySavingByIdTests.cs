using FinanceOne.Api.Features.MonthlySavings.GetMonthlySavingById;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.MonthlySavings.GetMonthlySavingById;

public class GetMonthlySavingByIdTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetMonthlySavingByIdHandler Handler => new(new GetMonthlySavingByIdRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Monthly_Saving_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetMonthlySavingByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Monthly_Saving_With_Its_Goal_Name()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var monthlySaving = await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        var response = await Handler.Handle(
            new GetMonthlySavingByIdQuery(monthlySaving.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var vm = response.Result!;
        Assert.Equal(monthlySaving.Id, vm.Id);
        Assert.Equal("Car Fund", vm.Name);
        Assert.Equal(5_000m, vm.Amount);
        Assert.Equal(goal.Id, vm.SavingGoalId);
        Assert.Equal("New Car", vm.SavingGoalName);
        Assert.Equal(25, vm.RecurrenceDay);
    }
}
