using FinanceOne.Api.Features.Expenses.CreateExpense;

namespace FinanceOne.UnitTests.Features.Expenses.CreateExpense;

public class CreateExpenseHandlerTests
{
    private readonly ICreateExpenseRepository _repository = Substitute.For<ICreateExpenseRepository>();

    private CreateExpenseHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        _repository.GetExpenseCategory(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var response = await Handler.Handle(
            new CreateExpenseCommand("Rent", 12_000m, Guid.NewGuid(), 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
    }

    // The FK only requires *a* category; "must be an Expense category" is an application rule, so
    // it's the handler's job to reject an Income one.
    [Fact]
    public async Task Returns_404_When_The_Category_Is_An_Income_Category()
    {
        var categoryId = Guid.NewGuid();
        _repository.GetExpenseCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Salary", Type = CategoryType.Income });

        var response = await Handler.Handle(
            new CreateExpenseCommand("Rent", 12_000m, categoryId, 1), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creates_The_Expense_And_Returns_Its_Id()
    {
        var categoryId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        _repository.GetExpenseCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Rent", Type = CategoryType.Expense });
        _repository.Add(Arg.Any<Expense>(), Arg.Any<CancellationToken>()).Returns(newId);

        var response = await Handler.Handle(
            new CreateExpenseCommand("Monthly Rent", 12_000m, categoryId, 5), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(newId, response.Result);
        await _repository.Received(1).Add(
            Arg.Is<Expense>(e =>
                e.Name == "Monthly Rent"
                && e.Amount == 12_000m
                && e.CategoryId == categoryId
                && e.RecurrenceDay == 5),
            Arg.Any<CancellationToken>());
    }
}
