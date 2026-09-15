using FinanceOne.Api.Features.UpcomingPayments.GetUpcomingPayments;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.UpcomingPayments.GetUpcomingPayments;

// This slice owns no table and does all its work in the handler: it walks forward day by day from
// today and matches each recurring income/expense by its day-of-month. Every rule below lives in
// that loop, so this is the one place they can be pinned down.
public class GetUpcomingPaymentsHandlerTests
{
    // A Monday mid-month, far enough from either end that a 7-day window stays inside June.
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly IGetUpcomingPaymentsRepository _repository =
        Substitute.For<IGetUpcomingPaymentsRepository>();

    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetUpcomingPaymentsHandler Handler => new(_repository, _timeProvider);

    private void Given(
        List<(string Name, decimal Amount, int RecurrenceDay)>? incomes = null,
        List<(string Name, decimal Amount, int RecurrenceDay)>? expenses = null)
    {
        _repository.GetRecurringIncomes(Arg.Any<CancellationToken>()).Returns(incomes ?? []);
        _repository.GetRecurringExpenses(Arg.Any<CancellationToken>()).Returns(expenses ?? []);
    }

    [Fact]
    public async Task Returns_An_Empty_List_When_Nothing_Recurs()
    {
        Given();

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    // The window is [today, today + days), so with the default of 7 the 21st is the last day
    // included and the 22nd is already outside.
    [Fact]
    public async Task Defaults_To_A_Seven_Day_Window_Starting_Today()
    {
        Given(expenses:
        [
            ("Today", 100m, 15),
            ("Last day in window", 200m, 21),
            ("Just outside", 300m, 22),
        ]);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(null), CancellationToken.None);

        Assert.Equal(["Today", "Last day in window"], response.Result!.Select(p => p.Name));
    }

    [Fact]
    public async Task Honours_An_Explicit_Window()
    {
        Given(expenses: [("In 10 days", 100m, 25)]);

        var shortWindow = await Handler.Handle(new GetUpcomingPaymentsQuery(7), CancellationToken.None);
        var longWindow = await Handler.Handle(new GetUpcomingPaymentsQuery(14), CancellationToken.None);

        Assert.Empty(shortWindow.Result!);
        Assert.Single(longWindow.Result!);
    }

    // `days is > 0` means zero and negatives fall back to the 7-day default rather than producing
    // an empty list or looping backwards.
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Falls_Back_To_Seven_Days_For_A_Non_Positive_Window(int days)
    {
        Given(expenses: [("Last day in default window", 200m, 21)]);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(days), CancellationToken.None);

        Assert.Single(response.Result!);
    }

    [Fact]
    public async Task Dates_Each_Payment_By_Its_Recurrence_Day()
    {
        Given(expenses: [("Rent", 12_000m, 18)]);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(null), CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 6, 18), Assert.Single(response.Result!).Date);
    }

    [Fact]
    public async Task Tags_Incomes_And_Expenses_With_Their_Type()
    {
        Given(
            incomes: [("Salary", 45_000m, 16)],
            expenses: [("Rent", 12_000m, 17)]);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(null), CancellationToken.None);

        Assert.Equal(CategoryType.Income, response.Result!.Single(p => p.Name == "Salary").Type);
        Assert.Equal(CategoryType.Expense, response.Result!.Single(p => p.Name == "Rent").Type);
    }

    [Fact]
    public async Task Orders_By_Date()
    {
        Given(
            incomes: [("Salary", 45_000m, 20)],
            expenses: [("Rent", 12_000m, 16), ("Gym", 499m, 18)]);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(null), CancellationToken.None);

        Assert.Equal(["Rent", "Gym", "Salary"], response.Result!.Select(p => p.Name));
    }

    // The window can cross into next month, and a recurrence day is matched against each date's
    // own day-of-month — so a day-1 expense shows up on the 1st of July, not back in June.
    [Fact]
    public async Task Rolls_Into_The_Next_Month()
    {
        _timeProvider.SetUtcNow(new DateTimeOffset(2026, 6, 28, 10, 0, 0, TimeSpan.Zero));
        Given(expenses: [("Rent", 12_000m, 1)]);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(7), CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 7, 1), Assert.Single(response.Result!).Date);
    }

    // A window longer than a month makes the same recurrence day come round twice, and both
    // occurrences are listed.
    [Fact]
    public async Task Lists_A_Payment_Once_Per_Occurrence_In_A_Long_Window()
    {
        Given(expenses: [("Rent", 12_000m, 20)]);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(45), CancellationToken.None);

        Assert.Equal(
            [new DateOnly(2026, 6, 20), new DateOnly(2026, 7, 20)],
            response.Result!.Select(p => p.Date));
    }
}
