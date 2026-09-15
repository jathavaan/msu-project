using FinanceOne.Api.Features.Expenses.UpdateExpense;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Expenses.UpdateExpense;

public class UpdateExpenseTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateExpenseHandler Handler => new(new UpdateExpenseRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Expense_Does_Not_Exist()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(
            new UpdateExpenseCommand(Guid.NewGuid(), "Rent", 12_000m, category.Id, 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_When_The_Target_Category_Does_Not_Exist()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var expense = await GivenExpense(category.Id, "Monthly Rent", 12_000m, 1);

        var response = await Handler.Handle(
            new UpdateExpenseCommand(expense.Id, "Monthly Rent", 12_000m, Guid.NewGuid(), 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_When_The_Target_Category_Is_An_Income_Category()
    {
        var expenseCategory = await GivenCategory("Rent", CategoryType.Expense);
        var incomeCategory = await GivenCategory("Salary", CategoryType.Income);
        var expense = await GivenExpense(expenseCategory.Id, "Monthly Rent", 12_000m, 1);

        var response = await Handler.Handle(
            new UpdateExpenseCommand(expense.Id, "Monthly Rent", 12_000m, incomeCategory.Id, 1),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);

        await using var context = NewContext();
        Assert.Equal(expenseCategory.Id, (await context.Expenses.SingleAsync(e => e.Id == expense.Id)).CategoryId);
    }

    [Fact]
    public async Task Persists_Every_Changed_Field()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        var expense = await GivenExpense(category.Id, "Monthly Rent", 12_000m, 1);

        var response = await Handler.Handle(
            new UpdateExpenseCommand(expense.Id, "Rent (incl. parking)", 13_200m, category.Id, 5),
            CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Expenses.SingleAsync(e => e.Id == expense.Id);
        Assert.Equal("Rent (incl. parking)", saved.Name);
        Assert.Equal(13_200m, saved.Amount);
        Assert.Equal(5, saved.RecurrenceDay);
    }

    // Moving an expense between categories is what makes the budget "used this month" figure shift,
    // so the new FK has to actually land in the database.
    [Fact]
    public async Task Moves_The_Expense_To_Another_Expense_Category()
    {
        var from = await GivenCategory("Shopping", CategoryType.Expense);
        var to = await GivenCategory("Health", CategoryType.Expense);
        var expense = await GivenExpense(from.Id, "Pharmacy", 450m, 12);

        await Handler.Handle(
            new UpdateExpenseCommand(expense.Id, "Pharmacy", 450m, to.Id, 12), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(to.Id, (await context.Expenses.SingleAsync(e => e.Id == expense.Id)).CategoryId);
    }
}
