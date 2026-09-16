using FinanceOne.Api.Features.Budgets.GetBudgets;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Budgets.GetBudgets;

public class GetBudgetsTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetBudgetsHandler Handler => new(new GetBudgetsRepository(Context));

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_No_Budgets()
    {
        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Includes_The_Category_Name_From_The_Joined_Category()
    {
        var category = await GivenCategory("Rent", CategoryType.Expense);
        await GivenBudget(category.Id, 12_000m);

        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        var budget = Assert.Single(response.Result!);
        Assert.Equal("Rent", budget.CategoryName);
        Assert.Equal(12_000m, budget.MonthlyLimit);
    }

    // Expenses are recurring templates, not a dated transaction log, so "used this month" sums
    // every expense in the category regardless of which day of the month it recurs on — including
    // one due later than today. This is the rule the SQL subquery encodes and the one an
    // in-memory provider would happily get wrong.
    [Fact]
    public async Task Counts_All_Expenses_Regardless_Of_Recurrence_Day()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);
        await GivenBudget(category.Id, 1_000m);
        await GivenExpense(category.Id, "Netflix", 149m, recurrenceDay: 1);
        await GivenExpense(category.Id, "Spotify", 119m, recurrenceDay: 15);
        await GivenExpense(category.Id, "Gym", 499m, recurrenceDay: 28);

        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        var budget = Assert.Single(response.Result!);
        Assert.Equal(767m, budget.UsedThisMonth);
    }

    [Fact]
    public async Task Reports_Zero_Usage_When_The_Category_Has_No_Expenses_Yet()
    {
        var category = await GivenCategory("Health", CategoryType.Expense);
        await GivenBudget(category.Id, 1_000m);

        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        var budget = Assert.Single(response.Result!);
        Assert.Equal(0m, budget.UsedThisMonth);
    }

    [Fact]
    public async Task Does_Not_Count_Expenses_Belonging_To_Another_Category()
    {
        var budgeted = await GivenCategory("Transport", CategoryType.Expense);
        var other = await GivenCategory("Shopping", CategoryType.Expense);
        await GivenBudget(budgeted.Id, 2_000m);
        await GivenExpense(budgeted.Id, "Bus Pass", 800m, recurrenceDay: 1);
        await GivenExpense(other.Id, "New Shoes", 1_500m, recurrenceDay: 1);

        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        var budget = Assert.Single(response.Result!);
        Assert.Equal(800m, budget.UsedThisMonth);
    }

    [Fact]
    public async Task Returns_Every_Budget()
    {
        var rent = await GivenCategory("Rent", CategoryType.Expense);
        var food = await GivenCategory("Food & Drinks", CategoryType.Expense);
        await GivenBudget(rent.Id, 12_000m);
        await GivenBudget(food.Id, 5_000m);

        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        Assert.Equal(2, response.Result!.Count);
    }
}
