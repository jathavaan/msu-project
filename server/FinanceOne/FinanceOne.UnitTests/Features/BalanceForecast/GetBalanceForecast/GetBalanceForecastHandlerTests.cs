using FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

namespace FinanceOne.UnitTests.Features.BalanceForecast.GetBalanceForecast;

// This slice owns no table and does all its work in the handler: it walks a 28-day period,
// starting from the configured period start day and wrapping, applying each recurring
// income/expense/saving by its recurrence day to a running balance. The rollover rule below — the
// balance no longer starts at 0 — lives entirely in that loop, so this is the one place it can be
// pinned down.
public class GetBalanceForecastHandlerTests
{
    private readonly IGetBalanceForecastRepository _repository =
        Substitute.For<IGetBalanceForecastRepository>();

    private GetBalanceForecastHandler Handler => new(_repository);

    private void Given(
        List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>? incomes = null,
        List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>? expenses = null,
        List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>? savings = null,
        int? periodStartDay = null)
    {
        _repository.GetRecurringIncomes(Arg.Any<CancellationToken>()).Returns(incomes ?? []);
        _repository.GetRecurringExpenses(Arg.Any<CancellationToken>()).Returns(expenses ?? []);
        _repository.GetRecurringMonthlySavings(Arg.Any<CancellationToken>()).Returns(savings ?? []);
        _repository.GetPeriodStartDay(Arg.Any<CancellationToken>()).Returns(periodStartDay);
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

    // The rollover rule: every month has the same recurring income/expenses/savings, so last month
    // ended `totalNet` above where it started. That rolled-over total is this month's starting
    // balance too — day 1 starts at `totalNet` instead of 0.
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

    // A monthly saving leaving the account is treated like an expense: it dips the running balance
    // (and the rolled-over starting balance) on its own recurrence day.
    [Fact]
    public async Task Monthly_Savings_Dip_The_Balance_Like_An_Expense()
    {
        Given(savings: [("Buffer account", "Buffer", 4_000m, 26)]);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var totalNet = -4_000m;
        Assert.Equal(totalNet, response.Result![0].Balance);
        // Day 25: still before the saving lands.
        Assert.Equal(totalNet, response.Result![24].Balance);
        // Day 26: the saving lands.
        Assert.Equal(totalNet - 4_000m, response.Result![25].Balance);
    }

    [Fact]
    public async Task Breaks_Savings_Down_Under_The_Linked_Saving_Goal_Name()
    {
        Given(savings: [("Buffer account", "Buffer", 4_000m, 26)]);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var day26 = response.Result![25];
        var entry = Assert.Single(day26.Savings);
        Assert.Equal("Buffer account", entry.Name);
        Assert.Equal("Buffer", entry.CategoryName);
        Assert.Equal(4_000m, entry.Amount);
    }

    // With no AppSettings row, GetPeriodStartDay returns null and the handler falls back to day 1
    // — the walk stays exactly the old always-day-1 behavior.
    [Fact]
    public async Task Defaults_To_Starting_On_Day_One_When_No_Period_Start_Day_Is_Configured()
    {
        Given(periodStartDay: null, expenses: [("Rent", "Housing", 12_000m, 5)]);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        Assert.Equal(1, response.Result![0].Day);
        Assert.Equal(5, response.Result![4].Day);
        Assert.Equal(28, response.Result![27].Day);
    }

    // A configured start day (e.g. 25, a payday) shifts and wraps the walk instead of always
    // starting at day 1: 25, 26, 27, 28, 1, 2, ... 24.
    [Fact]
    public async Task Custom_Start_Day_Wraps_The_28_Day_Walk_Across_The_Month_Boundary()
    {
        Given(periodStartDay: 25);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var days = response.Result!.Select(p => p.Day).ToList();
        List<int> expected = [25, 26, 27, 28, .. Enumerable.Range(1, 24)];
        Assert.Equal(expected, days);
    }

    [Fact]
    public async Task Custom_Start_Day_Still_Applies_Entries_On_Their_Own_Recurrence_Day()
    {
        Given(periodStartDay: 25, incomes: [("Salary", "Salary", 45_000m, 25)]);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        // Day 25 is now the first point in the walk, so the salary lands immediately — on top of
        // the rolled-over totalNet, which already includes this same salary once (see the rollover
        // rule pinned down above), so the balance becomes 2 * totalNet, not totalNet.
        Assert.Equal(25, response.Result![0].Day);
        Assert.Equal(90_000m, response.Result![0].Balance);
        Assert.Equal("Salary", Assert.Single(response.Result![0].Incomes).Name);
    }
}
