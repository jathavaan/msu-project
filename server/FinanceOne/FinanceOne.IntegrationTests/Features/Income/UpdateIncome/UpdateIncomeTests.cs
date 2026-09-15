using FinanceOne.Api.Features.Income.UpdateIncome;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Income.UpdateIncome;

public class UpdateIncomeTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateIncomeHandler Handler => new(new UpdateIncomeRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Income_Does_Not_Exist()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);

        var response = await Handler.Handle(
            new UpdateIncomeCommand(Guid.NewGuid(), "Salary", 45_000m, category.Id, 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_When_The_Target_Category_Does_Not_Exist()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);
        var income = await GivenIncome(category.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(
            new UpdateIncomeCommand(income.Id, "Monthly Salary", 45_000m, Guid.NewGuid(), 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_When_The_Target_Category_Is_An_Expense_Category()
    {
        var incomeCategory = await GivenCategory("Salary", CategoryType.Income);
        var expenseCategory = await GivenCategory("Rent", CategoryType.Expense);
        var income = await GivenIncome(incomeCategory.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(
            new UpdateIncomeCommand(income.Id, "Monthly Salary", 45_000m, expenseCategory.Id, 25),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);

        await using var context = NewContext();
        Assert.Equal(incomeCategory.Id, (await context.Incomes.SingleAsync(i => i.Id == income.Id)).CategoryId);
    }

    [Fact]
    public async Task Persists_Every_Changed_Field()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);
        var income = await GivenIncome(category.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(
            new UpdateIncomeCommand(income.Id, "Monthly Salary (after raise)", 48_500m, category.Id, 20),
            CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Incomes.SingleAsync(i => i.Id == income.Id);
        Assert.Equal("Monthly Salary (after raise)", saved.Name);
        Assert.Equal(48_500m, saved.Amount);
        Assert.Equal(20, saved.RecurrenceDay);
    }

    [Fact]
    public async Task Moves_The_Income_To_Another_Income_Category()
    {
        var from = await GivenCategory("Salary", CategoryType.Income);
        var to = await GivenCategory("Freelance", CategoryType.Income);
        var income = await GivenIncome(from.Id, "Side Project", 8_000m, 15);

        await Handler.Handle(
            new UpdateIncomeCommand(income.Id, "Side Project", 8_000m, to.Id, 15), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(to.Id, (await context.Incomes.SingleAsync(i => i.Id == income.Id)).CategoryId);
    }
}
