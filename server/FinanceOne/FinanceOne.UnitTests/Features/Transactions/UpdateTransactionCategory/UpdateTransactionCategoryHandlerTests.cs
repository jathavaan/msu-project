using FinanceOne.Api.Features.Transactions.UpdateTransactionCategory;

namespace FinanceOne.UnitTests.Features.Transactions.UpdateTransactionCategory;

public class UpdateTransactionCategoryHandlerTests
{
    private readonly IUpdateTransactionCategoryRepository _repository = Substitute.For<IUpdateTransactionCategoryRepository>();

    private UpdateTransactionCategoryHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Transaction_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var response = await Handler.Handle(
            new UpdateTransactionCategoryCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var transaction = new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 1), Description = "REMA 1000", Amount = -150m, Hash = "h" };
        _repository.GetById(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        _repository.GetCategory(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var response = await Handler.Handle(
            new UpdateTransactionCategoryCommand(transaction.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Assigns_The_Category()
    {
        var categoryId = Guid.NewGuid();
        var transaction = new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 1), Description = "REMA 1000", Amount = -150m, Hash = "h" };
        _repository.GetById(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        _repository.GetCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Food", Type = CategoryType.Expense });

        var response = await Handler.Handle(
            new UpdateTransactionCategoryCommand(transaction.Id, categoryId), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(categoryId, transaction.CategoryId);
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Null_CategoryId_Clears_The_Category_Without_Looking_One_Up()
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 1), Description = "REMA 1000", Amount = -150m, Hash = "h", CategoryId = Guid.NewGuid()
        };
        _repository.GetById(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        var response = await Handler.Handle(
            new UpdateTransactionCategoryCommand(transaction.Id, null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Null(transaction.CategoryId);
        await _repository.DidNotReceive().GetCategory(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }
}
