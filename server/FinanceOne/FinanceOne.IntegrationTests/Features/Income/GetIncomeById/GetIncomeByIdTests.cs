using FinanceOne.Api.Features.Income.GetIncomeById;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Income.GetIncomeById;

public class GetIncomeByIdTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetIncomeByIdHandler Handler => new(new GetIncomeByIdRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Income_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetIncomeByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Income_With_Its_Category_Name()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);
        var income = await GivenIncome(category.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(new GetIncomeByIdQuery(income.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var vm = response.Result!;
        Assert.Equal(income.Id, vm.Id);
        Assert.Equal("Monthly Salary", vm.Name);
        Assert.Equal(45_000m, vm.Amount);
        Assert.Equal(category.Id, vm.CategoryId);
        Assert.Equal("Salary", vm.CategoryName);
        Assert.Equal(25, vm.RecurrenceDay);
    }
}
