using FinanceOne.Api.Features.Budgets.GetBudgets;

namespace FinanceOne.UnitTests.Features.Budgets.GetBudgets;

public class GetBudgetsHandlerTests
{
    private readonly IGetBudgetsRepository _repository = Substitute.For<IGetBudgetsRepository>();

    private GetBudgetsHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_What_The_Repository_Produced()
    {
        var budgets = new List<BudgetVm>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Rent", 12_000m, 12_000m),
            new(Guid.NewGuid(), Guid.NewGuid(), "Food & Drinks", 5_000m, 1_250m),
        };
        _repository.GetBudgetsWithUsage(Arg.Any<CancellationToken>()).Returns(budgets);

        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(budgets, response.Result);
    }

    // An empty list is a successful result, not a 404 — the frontend renders its own empty state.
    [Fact]
    public async Task Returns_Success_With_An_Empty_List_When_There_Are_No_Budgets()
    {
        _repository.GetBudgetsWithUsage(Arg.Any<CancellationToken>()).Returns([]);

        var response = await Handler.Handle(new GetBudgetsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }
}
