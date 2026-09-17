using FinanceOne.Api.Features.Transactions.UpdateTransactionCategory;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Transactions.UpdateTransactionCategory;

public class UpdateTransactionCategoryTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateTransactionCategoryHandler Handler => new(new UpdateTransactionCategoryRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Transaction_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new UpdateTransactionCategoryCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var transaction = await GivenTransaction(new DateOnly(2026, 6, 1), "Unknown", -20m, "hash-1");

        var response = await Handler.Handle(
            new UpdateTransactionCategoryCommand(transaction.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Persists_The_Assigned_Category()
    {
        var transaction = await GivenTransaction(new DateOnly(2026, 6, 1), "Unknown", -20m, "hash-1");
        var food = await GivenCategory("Food", CategoryType.Expense);

        var response = await Handler.Handle(
            new UpdateTransactionCategoryCommand(transaction.Id, food.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Transactions.SingleAsync(t => t.Id == transaction.Id);
        Assert.Equal(food.Id, saved.CategoryId);
    }

    [Fact]
    public async Task Persists_Clearing_The_Category_Back_To_Uncategorized()
    {
        var food = await GivenCategory("Food", CategoryType.Expense);
        var transaction = await GivenTransaction(new DateOnly(2026, 6, 1), "REMA 1000", -150m, "hash-1", food.Id);

        var response = await Handler.Handle(
            new UpdateTransactionCategoryCommand(transaction.Id, null), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.Transactions.SingleAsync(t => t.Id == transaction.Id);
        Assert.Null(saved.CategoryId);
    }
}
