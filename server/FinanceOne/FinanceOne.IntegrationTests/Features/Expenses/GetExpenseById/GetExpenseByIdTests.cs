using FinanceOne.Api.Features.Expenses.GetExpenseById;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Expenses.GetExpenseById;

public class GetExpenseByIdTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetExpenseByIdHandler Handler => new(new GetExpenseByIdRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Expense_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetExpenseByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Expense_With_Its_Category_Name()
    {
        var category = await GivenCategory("Utilities", CategoryType.Expense);
        var expense = await GivenExpense(category.Id, "Electricity Bill", 1_800m, 5);

        var response = await Handler.Handle(new GetExpenseByIdQuery(expense.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        var vm = response.Result!;
        Assert.Equal(expense.Id, vm.Id);
        Assert.Equal("Electricity Bill", vm.Name);
        Assert.Equal(1_800m, vm.Amount);
        Assert.Equal(category.Id, vm.CategoryId);
        Assert.Equal("Utilities", vm.CategoryName);
        Assert.Equal(5, vm.RecurrenceDay);
    }
}
