using FinanceOne.Api.Features.Budgets.CreateBudget;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Budgets.CreateBudget;

public class CreateBudgetTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private CreateBudgetHandler Handler => new(new CreateBudgetRepository(Context));

    [Fact]
    public async Task Persists_The_Budget_Against_The_Expense_Category()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);

        var response = await Handler.Handle(new CreateBudgetCommand(category.Id, 12_000m), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Budgets.SingleAsync(b => b.Id == response.Result);
        Assert.Equal(category.Id, saved.CategoryId);
        Assert.Equal(12_000m, saved.MonthlyLimit);
    }

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var response = await Handler.Handle(new CreateBudgetCommand(Guid.NewGuid(), 12_000m), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await using var context = NewContext();
        Assert.Empty(await context.Budgets.ToListAsync());
    }

    [Fact]
    public async Task Returns_404_When_The_Category_Is_An_Income_Category()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);

        var response = await Handler.Handle(new CreateBudgetCommand(category.Id, 12_000m), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // BudgetConfiguration maps this one-to-one with HasForeignKey<Budget>, which puts a unique
    // index on CategoryId. The 409 exists so the second attempt is a clean business failure rather
    // than a DbUpdateException surfacing as a 500.
    [Fact]
    public async Task Returns_409_When_The_Category_Already_Has_A_Budget()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        await GivenBudget(category.Id, 12_000m);

        var response = await Handler.Handle(new CreateBudgetCommand(category.Id, 9_000m), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);

        await using var context = NewContext();
        var budget = await context.Budgets.SingleAsync(b => b.CategoryId == category.Id);
        Assert.Equal(12_000m, budget.MonthlyLimit);
    }

    // MonthlyLimit is decimal(18,2) in MySQL. Anything finer is rounded on write, so a caller can
    // never read back a value the database did not actually store.
    [Fact]
    public async Task Rounds_The_Monthly_Limit_To_Two_Decimal_Places()
    {
        var category = await GivenCategory("Food & Drinks", CategoryType.Expense);

        var response = await Handler.Handle(new CreateBudgetCommand(category.Id, 1_234.567m), CancellationToken.None);

        await using var context = NewContext();
        var saved = await context.Budgets.SingleAsync(b => b.Id == response.Result);
        Assert.Equal(1_234.57m, saved.MonthlyLimit);
    }
}
