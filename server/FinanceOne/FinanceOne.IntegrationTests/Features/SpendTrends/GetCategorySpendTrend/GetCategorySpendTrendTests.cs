using FinanceOne.Api.Features.SpendTrends.GetCategorySpendTrend;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.SpendTrends.GetCategorySpendTrend;

// This slice owns no table: its repository reads Categories/Budgets for identity/limit and groups
// Transactions by year/month directly in the query. The GroupBy-by-date-parts + Sum shape is what's
// worth exercising against a real MySQL engine rather than an in-memory provider.
public class GetCategorySpendTrendTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetCategorySpendTrendHandler Handler =>
        new(new GetCategorySpendTrendRepository(Context), _timeProvider);

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetCategorySpendTrendQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_For_An_Income_Category()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(category.Id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Reports_Zero_For_Every_Month_When_Nothing_Was_Imported()
    {
        var category = await GivenCategory("Food", CategoryType.Expense);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(category.Id), CancellationToken.None);

        Assert.Equal(6, response.Result!.Months.Count);
        Assert.All(response.Result!.Months, m => Assert.Equal(0m, m.Actual));
    }

    [Fact]
    public async Task Sums_A_Months_Debit_Transactions_As_A_Positive_Spend_Figure()
    {
        var category = await GivenCategory("Food", CategoryType.Expense);
        await GivenTransaction(new DateOnly(2026, 6, 3), "Grocery Store", -450.75m, "h1", category.Id);
        await GivenTransaction(new DateOnly(2026, 6, 10), "Restaurant", -120.25m, "h2", category.Id);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(category.Id), CancellationToken.None);

        Assert.Equal(571.00m, response.Result!.Months.Single(m => m.Year == 2026 && m.Month == 6).Actual);
    }

    [Fact]
    public async Task Nets_A_Refund_Against_The_Same_Months_Spend()
    {
        var category = await GivenCategory("Food", CategoryType.Expense);
        await GivenTransaction(new DateOnly(2026, 6, 3), "Grocery Store", -450m, "h1", category.Id);
        await GivenTransaction(new DateOnly(2026, 6, 4), "Refund", 50m, "h2", category.Id);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(category.Id), CancellationToken.None);

        Assert.Equal(400m, response.Result!.Months.Single(m => m.Month == 6).Actual);
    }

    [Fact]
    public async Task Excludes_Transactions_Older_Than_The_Six_Month_Window()
    {
        var category = await GivenCategory("Food", CategoryType.Expense);
        await GivenTransaction(new DateOnly(2025, 12, 20), "Old Purchase", -999m, "h1", category.Id);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(category.Id), CancellationToken.None);

        Assert.All(response.Result!.Months, m => Assert.Equal(0m, m.Actual));
    }

    [Fact]
    public async Task Excludes_Transactions_From_A_Different_Category()
    {
        var food = await GivenCategory("Food", CategoryType.Expense);
        var transport = await GivenCategory("Transport", CategoryType.Expense);
        await GivenTransaction(new DateOnly(2026, 6, 3), "Bus Pass", -300m, "h1", transport.Id);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(food.Id), CancellationToken.None);

        Assert.All(response.Result!.Months, m => Assert.Equal(0m, m.Actual));
    }

    [Fact]
    public async Task Includes_The_Budget_Limit_When_One_Exists()
    {
        var category = await GivenCategory("Food", CategoryType.Expense);
        await GivenBudget(category.Id, 5000m);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(category.Id), CancellationToken.None);

        Assert.Equal(5000m, response.Result!.MonthlyLimit);
    }

    [Fact]
    public async Task Returns_A_Null_Limit_When_The_Category_Has_No_Budget()
    {
        var category = await GivenCategory("Food", CategoryType.Expense);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(category.Id), CancellationToken.None);

        Assert.Null(response.Result!.MonthlyLimit);
    }
}
