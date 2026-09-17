using FinanceOne.Api.Features.Transactions.GetTransactions;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Transactions.GetTransactions;

public class GetTransactionsTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetTransactionsHandler Handler => new(new GetTransactionsRepository(Context));

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_No_Transactions()
    {
        var response = await Handler.Handle(new GetTransactionsQuery(null, false, null, null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Orders_By_Date_Descending_And_Includes_The_Category_Name()
    {
        var food = await GivenCategory("Food", CategoryType.Expense);
        await GivenTransaction(new DateOnly(2026, 6, 1), "REMA 1000", -150m, "hash-1", food.Id);
        await GivenTransaction(new DateOnly(2026, 6, 15), "Kiwi", -80m, "hash-2", food.Id);

        var response = await Handler.Handle(new GetTransactionsQuery(null, false, null, null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(["Kiwi", "REMA 1000"], response.Result!.Select(t => t.Description));
        Assert.All(response.Result!, t => Assert.Equal("Food", t.CategoryName));
    }

    [Fact]
    public async Task Filters_To_Uncategorized_Only()
    {
        var food = await GivenCategory("Food", CategoryType.Expense);
        await GivenTransaction(new DateOnly(2026, 6, 1), "REMA 1000", -150m, "hash-1", food.Id);
        await GivenTransaction(new DateOnly(2026, 6, 2), "Unknown", -20m, "hash-2");

        var response = await Handler.Handle(new GetTransactionsQuery(null, true, null, null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(["Unknown"], response.Result!.Select(t => t.Description));
        Assert.Null(response.Result![0].CategoryId);
    }

    [Fact]
    public async Task Filters_By_Date_Range()
    {
        await GivenTransaction(new DateOnly(2026, 5, 31), "Before range", -10m, "hash-1");
        await GivenTransaction(new DateOnly(2026, 6, 15), "In range", -10m, "hash-2");
        await GivenTransaction(new DateOnly(2026, 7, 1), "After range", -10m, "hash-3");

        var response = await Handler.Handle(
            new GetTransactionsQuery(null, false, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(["In range"], response.Result!.Select(t => t.Description));
    }
}
