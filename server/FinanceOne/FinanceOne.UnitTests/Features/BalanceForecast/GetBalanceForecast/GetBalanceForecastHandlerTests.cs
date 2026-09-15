using FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

namespace FinanceOne.UnitTests.Features.BalanceForecast.GetBalanceForecast;

// This slice owns no table and does all its work in the handler: it walks days 1-28 applying each
// recurring income/expense by its recurrence day to a running balance. The rollover rule below —
// the balance no longer starts at 0 — lives entirely in that loop, so this is the one place it can
// be pinned down.
public class GetBalanceForecastHandlerTests
{
    private readonly IGetBalanceForecastRepository _repository =
        Substitute.For<IGetBalanceForecastRepository>();

    private GetBalanceForecastHandler Handler => new(_repository);

    private void Given(
        List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>? incomes = null,
        List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>? expenses = null)
    {
        _repository.GetRecurringIncomes(Arg.Any<CancellationToken>()).Returns(incomes ?? []);
        _repository.GetRecurringExpenses(Arg.Any<CancellationToken>()).Returns(expenses ?? []);
    }

    [Fact]
    public async Task Returns_Twenty_Eight_Points_Starting_At_Zero_When_Nothing_Recurs()
    {
        Given();

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(28, response.Result!.Count);
        Assert.All(response.Result!, point => Assert.Equal(0m, point.Balance));
    }

    // The rollover rule: every month has the same recurring income/expenses, so last month ended
    // `totalNet` above where it started. That rolled-over total is this month's starting balance
    // too — day 1 starts at `totalNet` instead of 0.
    [Fact]
    public async Task Day_One_Starts_At_The_Rolled_Over_Total_Net_Instead_Of_Zero()
    {
        Given(
            incomes: [("Salary", "Salary", 45_000m, 20)],
            expenses: [("Rent", "Housing", 12_000m, 5)]);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var totalNet = 45_000m - 12_000m;
        Assert.Equal(totalNet, response.Result![0].Balance);
    }

    [Fact]
    public async Task Day_Twenty_Eight_Ends_At_Twice_The_Total_Net()
    {
        Given(
            incomes: [("Salary", "Salary", 45_000m, 20)],
            expenses: [("Rent", "Housing", 12_000m, 5)]);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var totalNet = 45_000m - 12_000m;
        Assert.Equal(totalNet * 2, response.Result![27].Balance);
    }

    [Fact]
    public async Task Balance_Moves_On_The_Day_Each_Occurrence_Falls_Due()
    {
        Given(
            incomes: [("Salary", "Salary", 45_000m, 20)],
            expenses: [("Rent", "Housing", 12_000m, 5)]);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var totalNet = 45_000m - 12_000m;
        // Days 1-4: still just the rolled-over starting balance.
        Assert.Equal(totalNet, response.Result![0].Balance);
        Assert.Equal(totalNet, response.Result![3].Balance);
        // Day 5: rent lands.
        Assert.Equal(totalNet - 12_000m, response.Result![4].Balance);
        // Day 19: still down the rent amount.
        Assert.Equal(totalNet - 12_000m, response.Result![18].Balance);
        // Day 20: salary lands.
        Assert.Equal(totalNet - 12_000m + 45_000m, response.Result![19].Balance);
    }

    [Fact]
    public async Task Breaks_Each_Day_Down_Into_The_Entries_Applied_That_Day()
    {
        Given(
            incomes: [("Salary", "Salary", 45_000m, 20)],
            expenses: [("Rent", "Housing", 12_000m, 5), ("Gym", "Subscriptions", 499m, 5)]);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var day5 = response.Result![4];
        Assert.Empty(day5.Incomes);
        Assert.Equal(["Rent", "Gym"], day5.Expenses.Select(e => e.Name));
        Assert.Equal("Housing", day5.Expenses[0].CategoryName);

        var day20 = response.Result![19];
        Assert.Equal("Salary", Assert.Single(day20.Incomes).Name);
        Assert.Empty(day20.Expenses);
    }
}
