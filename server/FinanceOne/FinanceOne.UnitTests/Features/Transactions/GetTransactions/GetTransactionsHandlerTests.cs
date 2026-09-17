using FinanceOne.Api.Features.Transactions.GetTransactions;

namespace FinanceOne.UnitTests.Features.Transactions.GetTransactions;

public class GetTransactionsHandlerTests
{
    private readonly IGetTransactionsRepository _repository = Substitute.For<IGetTransactionsRepository>();

    private GetTransactionsHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_What_The_Repository_Produced()
    {
        var transactions = new List<TransactionVm>
        {
            new(Guid.NewGuid(), new DateOnly(2026, 6, 1), "REMA 1000", -150m, Guid.NewGuid(), "Food"),
        };
        _repository.GetTransactions(null, false, null, null, Arg.Any<CancellationToken>()).Returns(transactions);

        var response = await Handler.Handle(new GetTransactionsQuery(null, false, null, null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(transactions, response.Result);
    }

    [Fact]
    public async Task Returns_Success_With_An_Empty_List_When_There_Are_No_Transactions()
    {
        _repository.GetTransactions(Arg.Any<Guid?>(), Arg.Any<bool>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await Handler.Handle(new GetTransactionsQuery(null, false, null, null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    [Fact]
    public async Task Passes_All_Filters_Through_Unchanged()
    {
        var categoryId = Guid.NewGuid();
        var from = new DateOnly(2026, 6, 1);
        var to = new DateOnly(2026, 6, 30);
        _repository.GetTransactions(Arg.Any<Guid?>(), Arg.Any<bool>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await Handler.Handle(new GetTransactionsQuery(categoryId, true, from, to), CancellationToken.None);

        await _repository.Received(1).GetTransactions(categoryId, true, from, to, Arg.Any<CancellationToken>());
    }
}
