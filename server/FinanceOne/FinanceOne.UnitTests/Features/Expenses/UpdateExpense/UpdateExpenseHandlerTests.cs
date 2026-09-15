using FinanceOne.Api.Features.Expenses.UpdateExpense;

namespace FinanceOne.UnitTests.Features.Expenses.UpdateExpense;

public class UpdateExpenseHandlerTests
{
    private readonly IUpdateExpenseRepository _repository = Substitute.For<IUpdateExpenseRepository>();

    private UpdateExpenseHandler Handler => new(_repository);

    private static Expense AnExpense(Guid id, Guid categoryId) => new()
    {
        Id = id,
        Name = "Rent",
        Amount = 12_000m,
        CategoryId = categoryId,
        RecurrenceDay = 1,
    };

    [Fact]
    public async Task Returns_404_When_The_Expense_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Expense?)null);

        var response = await Handler.Handle(
            new UpdateExpenseCommand(Guid.NewGuid(), "Rent", 12_000m, Guid.NewGuid(), 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    // An update can move an expense to a different category, so the target category is re-checked
    // on every update, not just at creation.
    [Fact]
    public async Task Returns_404_When_The_Target_Category_Does_Not_Exist()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(AnExpense(id, Guid.NewGuid()));
        _repository.GetExpenseCategory(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var response = await Handler.Handle(
            new UpdateExpenseCommand(id, "Rent", 12_000m, Guid.NewGuid(), 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_When_The_Target_Category_Is_An_Income_Category()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(AnExpense(id, Guid.NewGuid()));
        _repository.GetExpenseCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Salary", Type = CategoryType.Income });

        var response = await Handler.Handle(
            new UpdateExpenseCommand(id, "Rent", 12_000m, categoryId, 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Applies_Every_Field_Then_Saves()
    {
        var id = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var expense = AnExpense(id, Guid.NewGuid());
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(expense);
        _repository.GetExpenseCategory(newCategoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = newCategoryId, Name = "Utilities", Type = CategoryType.Expense });

        var response = await Handler.Handle(
            new UpdateExpenseCommand(id, "Electricity Bill", 1_800m, newCategoryId, 20), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("Electricity Bill", expense.Name);
        Assert.Equal(1_800m, expense.Amount);
        Assert.Equal(newCategoryId, expense.CategoryId);
        Assert.Equal(20, expense.RecurrenceDay);
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }
}
