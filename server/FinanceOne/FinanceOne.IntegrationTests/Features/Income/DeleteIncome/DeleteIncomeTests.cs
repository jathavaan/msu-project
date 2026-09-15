using FinanceOne.Api.Features.Income.DeleteIncome;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Income.DeleteIncome;

public class DeleteIncomeTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private DeleteIncomeHandler Handler => new(new DeleteIncomeRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Income_Does_Not_Exist()
    {
        var response = await Handler.Handle(new DeleteIncomeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Removes_The_Income_Row()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);
        var income = await GivenIncome(category.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(new DeleteIncomeCommand(income.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.False(await context.Incomes.AnyAsync(i => i.Id == income.Id));
    }

    // Deleting the last income in a category is what frees that category for deletion in turn
    // (see DeleteCategory's IsReferenced guard).
    [Fact]
    public async Task Leaves_The_Category_In_Place()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);
        var income = await GivenIncome(category.Id, "Monthly Salary", 45_000m, 25);

        await Handler.Handle(new DeleteIncomeCommand(income.Id), CancellationToken.None);

        await using var context = NewContext();
        Assert.True(await context.Categories.AnyAsync(c => c.Id == category.Id));
    }
}
