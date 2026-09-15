using FinanceOne.Api.Features.Expenses.GetExpenses;

namespace FinanceOne.UnitTests.Features.Expenses.GetExpenses;

public class GetExpensesHandlerTests
{
    private readonly IGetExpensesRepository _repository = Substitute.For<IGetExpensesRepository>();

    private GetExpensesHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_What_The_Repository_Produced()
    {
        var expenses = new List<ExpenseVm>
        {
            new(Guid.NewGuid(), "Monthly Rent", 12_000m, Guid.NewGuid(), "Rent", 1),
            new(Guid.NewGuid(), "Netflix Subscription", 149m, Guid.NewGuid(), "Subscriptions", 15),
        };
        _repository.GetExpenses(null, Arg.Any<CancellationToken>()).Returns(expenses);

        var response = await Handler.Handle(new GetExpensesQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(expenses, response.Result);
    }

    [Fact]
    public async Task Returns_Success_With_An_Empty_List_When_There_Are_No_Expenses()
    {
        _repository.GetExpenses(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns([]);

        var response = await Handler.Handle(new GetExpensesQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    // The optional ?categoryId= filter is passed straight through; null must stay null so the
    // repository knows to skip the filter rather than match Guid.Empty.
    [Fact]
    public async Task Passes_The_Category_Filter_Through_Unchanged()
    {
        var categoryId = Guid.NewGuid();
        _repository.GetExpenses(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns([]);

        await Handler.Handle(new GetExpensesQuery(categoryId), CancellationToken.None);
        await Handler.Handle(new GetExpensesQuery(null), CancellationToken.None);

        await _repository.Received(1).GetExpenses(categoryId, Arg.Any<CancellationToken>());
        await _repository.Received(1).GetExpenses(null, Arg.Any<CancellationToken>());
    }
}
