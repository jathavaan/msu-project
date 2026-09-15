using FinanceOne.Api.Features.Income.GetIncomes;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Income.GetIncomes;

public class GetIncomesTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetIncomesHandler Handler => new(new GetIncomesRepository(Context));

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_No_Incomes()
    {
        var response = await Handler.Handle(new GetIncomesQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Includes_The_Category_Name_From_The_Joined_Category()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);
        await GivenIncome(category.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(new GetIncomesQuery(), CancellationToken.None);

        var income = Assert.Single(response.Result!);
        Assert.Equal("Salary", income.CategoryName);
        Assert.Equal(45_000m, income.Amount);
        Assert.Equal(25, income.RecurrenceDay);
    }

    [Fact]
    public async Task Orders_By_Name()
    {
        var salary = await GivenCategory("Salary", CategoryType.Income);
        var freelance = await GivenCategory("Freelance", CategoryType.Income);
        await GivenIncome(salary.Id, "Overtime Pay", 4_000m, 25);
        await GivenIncome(freelance.Id, "Consulting Fee", 12_000m, 10);
        await GivenIncome(salary.Id, "Monthly Salary", 45_000m, 25);

        var response = await Handler.Handle(new GetIncomesQuery(), CancellationToken.None);

        Assert.Equal(["Consulting Fee", "Monthly Salary", "Overtime Pay"], response.Result!.Select(i => i.Name));
    }

    // GetIncomes takes no filter, so every income is returned regardless of category.
    [Fact]
    public async Task Returns_Incomes_Across_Every_Category()
    {
        var salary = await GivenCategory("Salary", CategoryType.Income);
        var gifts = await GivenCategory("Gifts", CategoryType.Income);
        await GivenIncome(salary.Id, "Monthly Salary", 45_000m, 25);
        await GivenIncome(gifts.Id, "Birthday Gift", 1_000m, 5);

        var response = await Handler.Handle(new GetIncomesQuery(), CancellationToken.None);

        Assert.Equal(2, response.Result!.Count);
    }
}
