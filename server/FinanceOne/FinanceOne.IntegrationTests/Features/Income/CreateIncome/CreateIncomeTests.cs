using FinanceOne.Api.Features.Income.CreateIncome;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Income.CreateIncome;

public class CreateIncomeTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private CreateIncomeHandler Handler => new(new CreateIncomeRepository(Context));

    [Fact]
    public async Task Persists_The_Income()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);

        var response = await Handler.Handle(
            new CreateIncomeCommand("Monthly Salary", 45_000m, category.Id, 25), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Incomes.SingleAsync(i => i.Id == response.Result);
        Assert.Equal("Monthly Salary", saved.Name);
        Assert.Equal(45_000m, saved.Amount);
        Assert.Equal(category.Id, saved.CategoryId);
        Assert.Equal(25, saved.RecurrenceDay);
    }

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new CreateIncomeCommand("Monthly Salary", 45_000m, Guid.NewGuid(), 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await using var context = NewContext();
        Assert.Empty(await context.Incomes.ToListAsync());
    }

    [Fact]
    public async Task Returns_404_When_The_Category_Is_An_Expense_Category()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(
            new CreateIncomeCommand("Monthly Salary", 45_000m, category.Id, 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Rounds_The_Amount_To_Two_Decimal_Places()
    {
        var category = await GivenCategory("Freelance", CategoryType.Income);

        var response = await Handler.Handle(
            new CreateIncomeCommand("Consulting Fee", 1_234.567m, category.Id, 10), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(1_234.57m, (await context.Incomes.SingleAsync(i => i.Id == response.Result)).Amount);
    }
}
