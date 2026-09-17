using FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.BalanceForecast.GetBalanceForecast;

// This slice owns no table: its repository reads Incomes and Expenses directly and projects each
// row into a ValueTuple via the Category navigation. That join and tuple projection is the part
// worth exercising against a real provider, not the day-walking logic covered by the unit tests.
public class GetBalanceForecastTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetBalanceForecastHandler Handler => new(new GetBalanceForecastRepository(Context));

    [Fact]
    public async Task Returns_Twenty_Eight_Points_Of_Zero_When_Nothing_Recurs()
    {
        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(28, response.Result!.Count);
        Assert.All(response.Result!, point => Assert.Equal(0m, point.Balance));
    }

    [Fact]
    public async Task Rolls_The_Total_Net_Over_As_The_Starting_Balance()
    {
        var salaryCategory = await GivenCategory("Salary", CategoryType.Income);
        var rentCategory = await GivenCategory("Rent", CategoryType.Expense);
        await GivenIncome(salaryCategory.Id, "Monthly Salary", 45_000m, 20);
        await GivenExpense(rentCategory.Id, "Monthly Rent", 12_000m, 5);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var totalNet = 45_000m - 12_000m;
        Assert.Equal(totalNet, response.Result![0].Balance);
        Assert.Equal(totalNet * 2, response.Result![27].Balance);
    }

    [Fact]
    public async Task Carries_The_Category_Name_Through_Its_Own_Join()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);
        await GivenExpense(category.Id, "Netflix", 149m, 16);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var day16 = response.Result![15];
        var entry = Assert.Single(day16.Expenses);
        Assert.Equal("Netflix", entry.Name);
        Assert.Equal("Subscriptions", entry.CategoryName);
    }

    [Fact]
    public async Task Carries_Amounts_Through_At_Full_Precision()
    {
        var category = await GivenCategory("Subscriptions", CategoryType.Expense);
        await GivenExpense(category.Id, "Phone Plan", 399.50m, 16);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        Assert.Equal(399.50m, Assert.Single(response.Result![15].Expenses).Amount);
    }

    [Fact]
    public async Task Monthly_Savings_Dip_The_Balance_And_Carry_The_Saving_Goal_Name_Through_Its_Own_Join()
    {
        var savingGoal = await GivenSavingGoal("Buffer", 75_000m, new DateOnly(2027, 1, 1));
        await GivenMonthlySaving(savingGoal.Id, "Buffer account", 4_000m, 26);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        var totalNet = -4_000m;
        Assert.Equal(totalNet, response.Result![0].Balance);
        var day26 = response.Result![25];
        Assert.Equal(totalNet - 4_000m, day26.Balance);
        var entry = Assert.Single(day26.Savings);
        Assert.Equal("Buffer account", entry.Name);
        Assert.Equal("Buffer", entry.CategoryName);
    }

    [Fact]
    public async Task Walks_From_The_Configured_Period_Start_Day_Instead_Of_Day_One()
    {
        await GivenAppSettings(periodStartDay: 25);
        var salaryCategory = await GivenCategory("Salary", CategoryType.Income);
        await GivenIncome(salaryCategory.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(new GetBalanceForecastQuery(), CancellationToken.None);

        List<int> expectedDays = [25, 26, 27, 28, .. Enumerable.Range(1, 24)];
        Assert.Equal(expectedDays, response.Result!.Select(p => p.Day));
        // Day 25 is the first point in the walk, so the salary lands immediately — on top of the
        // rolled-over totalNet, which already includes this same salary once, so the balance
        // becomes 2 * totalNet.
        Assert.Equal(90_000m, response.Result![0].Balance);
    }
}
