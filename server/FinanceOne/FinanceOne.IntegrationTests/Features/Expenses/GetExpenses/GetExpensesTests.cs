using FinanceOne.Api.Features.Expenses.GetExpenses;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Expenses.GetExpenses;

public class GetExpensesTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetExpensesHandler Handler => new(new GetExpensesRepository(Context));

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_No_Expenses()
    {
        var response = await Handler.Handle(new GetExpensesQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    // The Vm carries CategoryName, which only exists on the joined Category row — an expense whose
    // category name is missing would mean the join silently dropped.
    [Fact]
    public async Task Includes_The_Category_Name_From_The_Joined_Category()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        await GivenExpense(category.Id, "Monthly Rent", 12_000m, 1);

        var response = await Handler.Handle(new GetExpensesQuery(null), CancellationToken.None);

        var expense = Assert.Single(response.Result!);
        Assert.Equal("Rent", expense.CategoryName);
        Assert.Equal(12_000m, expense.Amount);
        Assert.Equal(1, expense.RecurrenceDay);
    }

    [Fact]
    public async Task Filters_By_Category()
    {
        var rent = await GivenCategory("Rent", CategoryType.Expense);
        var food = await GivenCategory("Food & Drinks", CategoryType.Expense);
        await GivenExpense(rent.Id, "Monthly Rent", 12_000m, 1);
        await GivenExpense(food.Id, "Grocery Shopping", 900m, 10);

        var response = await Handler.Handle(new GetExpensesQuery(rent.Id), CancellationToken.None);

        Assert.Equal("Monthly Rent", Assert.Single(response.Result!).Name);
    }

    [Fact]
    public async Task Returns_Every_Expense_When_No_Filter_Is_Given()
    {
        var rent = await GivenCategory("Rent", CategoryType.Expense);
        var food = await GivenCategory("Food & Drinks", CategoryType.Expense);
        await GivenExpense(rent.Id, "Monthly Rent", 12_000m, 1);
        await GivenExpense(food.Id, "Grocery Shopping", 900m, 10);

        var response = await Handler.Handle(new GetExpensesQuery(null), CancellationToken.None);

        Assert.Equal(2, response.Result!.Count);
    }

    [Fact]
    public async Task Orders_By_Name()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);
        await GivenExpense(category.Id, "Spotify", 119m, 1);
        await GivenExpense(category.Id, "Gym Membership", 499m, 1);
        await GivenExpense(category.Id, "Netflix", 149m, 1);

        var response = await Handler.Handle(new GetExpensesQuery(null), CancellationToken.None);

        Assert.Equal(["Gym Membership", "Netflix", "Spotify"], response.Result!.Select(e => e.Name));
    }

    // Filtering on a category that exists but has no expenses is an empty success, not a 404.
    [Fact]
    public async Task Returns_An_Empty_List_For_A_Category_With_No_Expenses()
    {
        var category = await GivenCategory("Health", CategoryType.Expense);

        var response = await Handler.Handle(new GetExpensesQuery(category.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }
}
