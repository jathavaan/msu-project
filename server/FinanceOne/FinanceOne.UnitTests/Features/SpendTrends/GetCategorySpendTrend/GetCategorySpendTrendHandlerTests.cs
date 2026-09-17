using FinanceOne.Api.Features.SpendTrends.GetCategorySpendTrend;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.SpendTrends.GetCategorySpendTrend;

public class GetCategorySpendTrendHandlerTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly IGetCategorySpendTrendRepository _repository =
        Substitute.For<IGetCategorySpendTrendRepository>();

    // Mid-month, same reasoning as GetUpcomingPaymentsHandlerTests: unambiguous which month is
    // "current" regardless of how many days it has.
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetCategorySpendTrendHandler Handler => new(_repository, _timeProvider);

    private void GivenCategory(string? name, CategoryType type = CategoryType.Expense, decimal? monthlyLimit = null) =>
        _repository.GetCategory(CategoryId, Arg.Any<CancellationToken>()).Returns((name, type, monthlyLimit));

    private void GivenMonthlySpend(params (int Year, int Month, decimal Total)[] totals) =>
        _repository.GetMonthlySpend(CategoryId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(totals.ToList());

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        GivenCategory(null);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(CategoryId), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().GetMonthlySpend(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_For_An_Income_Category()
    {
        GivenCategory("Salary", CategoryType.Income);

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(CategoryId), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().GetMonthlySpend(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_Six_Months_Oldest_First_Ending_On_The_Current_Month()
    {
        GivenCategory("Food");
        GivenMonthlySpend();

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(CategoryId), CancellationToken.None);

        Assert.Equal(
            [(2026, 1), (2026, 2), (2026, 3), (2026, 4), (2026, 5), (2026, 6)],
            response.Result!.Months.Select(m => (m.Year, m.Month)));
    }

    [Fact]
    public async Task Fills_A_Month_With_No_Transactions_As_Zero()
    {
        GivenCategory("Food");
        GivenMonthlySpend((2026, 3, 450m));

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(CategoryId), CancellationToken.None);

        Assert.Equal(450m, response.Result!.Months.Single(m => m.Month == 3).Actual);
        Assert.Equal(0m, response.Result!.Months.Single(m => m.Month == 1).Actual);
    }

    [Fact]
    public async Task Passes_Through_The_Budget_Limit_When_One_Exists()
    {
        GivenCategory("Food", monthlyLimit: 5000m);
        GivenMonthlySpend();

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(CategoryId), CancellationToken.None);

        Assert.Equal(5000m, response.Result!.MonthlyLimit);
    }

    [Fact]
    public async Task Returns_A_Null_Limit_When_The_Category_Has_No_Budget()
    {
        GivenCategory("Food");
        GivenMonthlySpend();

        var response = await Handler.Handle(new GetCategorySpendTrendQuery(CategoryId), CancellationToken.None);

        Assert.Null(response.Result!.MonthlyLimit);
    }

    // AddMonths(-(6-1)) from a June "today" lands on January — pins the fixed 6-month window down
    // to the exact month it queries from, not just the count of months returned.
    [Fact]
    public async Task Requests_Monthly_Spend_From_Five_Months_Before_The_Current_One()
    {
        GivenCategory("Food");
        GivenMonthlySpend();

        await Handler.Handle(new GetCategorySpendTrendQuery(CategoryId), CancellationToken.None);

        await _repository.Received(1).GetMonthlySpend(CategoryId, new DateOnly(2026, 1, 1), Arg.Any<CancellationToken>());
    }
}
