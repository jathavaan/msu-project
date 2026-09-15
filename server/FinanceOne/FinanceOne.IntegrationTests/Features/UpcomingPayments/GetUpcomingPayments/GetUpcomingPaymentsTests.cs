using FinanceOne.Api.Features.UpcomingPayments.GetUpcomingPayments;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.UpcomingPayments.GetUpcomingPayments;

// This slice owns no table: its repository reads Incomes and Expenses directly and projects each
// row into a ValueTuple. Tuple projection is the part worth exercising against a real provider —
// it is a query shape EF does not always translate, and an in-memory provider would accept it
// regardless of whether MySQL can.
public class GetUpcomingPaymentsTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetUpcomingPaymentsHandler Handler =>
        new(new GetUpcomingPaymentsRepository(Context), _timeProvider);

    [Fact]
    public async Task Returns_An_Empty_List_When_Nothing_Recurs()
    {
        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Combines_Incomes_And_Expenses_Into_One_Dated_Timeline()
    {
        var salaryCategory = await GivenCategory("Salary", CategoryType.Income);
        var rentCategory = await GivenCategory("Rent", CategoryType.Expense);
        await GivenIncome(salaryCategory.Id, "Monthly Salary", 45_000m, 20);
        await GivenExpense(rentCategory.Id, "Monthly Rent", 12_000m, 17);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(null), CancellationToken.None);

        Assert.Equal(["Monthly Rent", "Monthly Salary"], response.Result!.Select(p => p.Name));
        Assert.Equal(CategoryType.Expense, response.Result![0].Type);
        Assert.Equal(CategoryType.Income, response.Result![1].Type);
        Assert.Equal(new DateOnly(2026, 6, 17), response.Result![0].Date);
        Assert.Equal(new DateOnly(2026, 6, 20), response.Result![1].Date);
    }

    [Fact]
    public async Task Excludes_Anything_Outside_The_Window()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);
        await GivenExpense(category.Id, "Netflix", 149m, 16);
        await GivenExpense(category.Id, "Gym Membership", 499m, 28);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(7), CancellationToken.None);

        Assert.Equal("Netflix", Assert.Single(response.Result!).Name);
    }

    [Fact]
    public async Task Carries_Amounts_Through_At_Full_Precision()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);
        await GivenExpense(category.Id, "Phone Plan", 399.50m, 16);

        var response = await Handler.Handle(new GetUpcomingPaymentsQuery(null), CancellationToken.None);

        Assert.Equal(399.50m, Assert.Single(response.Result!).Amount);
    }
}
