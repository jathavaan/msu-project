using FinanceOne.Api.Features.Budgets.CreateBudget;

namespace FinanceOne.UnitTests.Features.Budgets.CreateBudget;

public class CreateBudgetHandlerTests
{
    private readonly ICreateBudgetRepository _repository = Substitute.For<ICreateBudgetRepository>();

    private CreateBudgetHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_Category_Does_Not_Exist()
    {
        _repository.GetExpenseCategory(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var response = await Handler.Handle(new CreateBudgetCommand(Guid.NewGuid(), 1_000m), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // A budget caps spending, so it only makes sense against an Expense category. The repository
    // deliberately fetches any category by id and lets the handler enforce the type rule.
    [Fact]
    public async Task Returns_404_When_Category_Is_An_Income_Category()
    {
        var categoryId = Guid.NewGuid();
        _repository.GetExpenseCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Salary", Type = CategoryType.Income });

        var response = await Handler.Handle(new CreateBudgetCommand(categoryId, 1_000m), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<Budget>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_409_When_Category_Already_Has_A_Budget()
    {
        var categoryId = Guid.NewGuid();
        _repository.GetExpenseCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Rent", Type = CategoryType.Expense });
        _repository.BudgetExistsForCategory(categoryId, Arg.Any<CancellationToken>()).Returns(true);

        var response = await Handler.Handle(new CreateBudgetCommand(categoryId, 1_000m), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<Budget>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creates_Budget_And_Returns_Its_Id()
    {
        var categoryId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        _repository.GetExpenseCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Rent", Type = CategoryType.Expense });
        _repository.BudgetExistsForCategory(categoryId, Arg.Any<CancellationToken>()).Returns(false);
        _repository.Add(Arg.Any<Budget>(), Arg.Any<CancellationToken>()).Returns(newId);

        var response = await Handler.Handle(new CreateBudgetCommand(categoryId, 12_500m), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(newId, response.Result);
        await _repository.Received(1).Add(
            Arg.Is<Budget>(b => b.CategoryId == categoryId && b.MonthlyLimit == 12_500m),
            Arg.Any<CancellationToken>());
    }
}
