using FinanceOne.Api.Features.Budgets.GetBudgetById;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.Budgets.GetBudgetById;

public class GetBudgetByIdTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetBudgetByIdHandler Handler => new(new GetBudgetByIdRepository(Context, _timeProvider));

    [Fact]
    public async Task Returns_404_When_The_Budget_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetBudgetByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Budget_With_Its_Category_Name_And_Usage()
    {
        var category = await GivenCategory("Utilities", CategoryType.Expense);
        var budget = await GivenBudget(category.Id, 3_000m);
        await GivenExpense(category.Id, "Electricity Bill", 1_200m, recurrenceDay: 5);
        await GivenExpense(category.Id, "Internet Bill", 599m, recurrenceDay: 20);

        var response = await Handler.Handle(new GetBudgetByIdQuery(budget.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(budget.Id, response.Result!.Id);
        Assert.Equal(category.Id, response.Result.CategoryId);
        Assert.Equal("Utilities", response.Result.CategoryName);
        Assert.Equal(3_000m, response.Result.MonthlyLimit);
        // Only the 5th has passed by the 15th; the 20th has not.
        Assert.Equal(1_200m, response.Result.UsedThisMonth);
    }
}
