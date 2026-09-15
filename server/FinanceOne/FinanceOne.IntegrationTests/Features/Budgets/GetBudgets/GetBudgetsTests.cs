using FinanceOne.Api.Features.Budgets.GetBudgets;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.Budgets.GetBudgets;

public class GetBudgetsTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    // The 15th of the month, so recurrence days on either side of "today" are both expressible.
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetBudgetsHandler Handler => new(new GetBudgetsRepository(Context, _timeProvider));

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

    // "Used this month" sums only the recurring expenses whose day has already passed, so an
    // expense due later in the month must not count yet. This is the rule the SQL subquery encodes
    // and the one an in-memory provider would happily get wrong.
    [Fact]
    public async Task Counts_Only_Expenses_Whose_Recurrence_Day_Has_Passed()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);
        await GivenBudget(category.Id, 1_000m);
        await GivenExpense(category.Id, "Netflix", 149m, recurrenceDay: 1);
        await GivenExpense(category.Id, "Spotify", 119m, recurrenceDay: 15);
        await GivenExpense(category.Id, "Gym", 499m, recurrenceDay: 28);

        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        var budget = Assert.Single(response.Result!);
        Assert.Equal(268m, budget.UsedThisMonth);
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
