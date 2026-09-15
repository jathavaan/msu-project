using FinanceOne.Api.Features.Income.CreateIncome;

namespace FinanceOne.UnitTests.Features.Income.CreateIncome;

public class CreateIncomeHandlerTests
{
    private readonly ICreateIncomeRepository _repository = Substitute.For<ICreateIncomeRepository>();

    private CreateIncomeHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        _repository.GetIncomeCategory(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var response = await Handler.Handle(
            new CreateIncomeCommand("Salary", 45_000m, Guid.NewGuid(), 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>());
    }

    // Mirror image of CreateExpense: income must land on an Income category, never an Expense one.
    [Fact]
    public async Task Returns_404_When_The_Category_Is_An_Expense_Category()
    {
        var categoryId = Guid.NewGuid();
        _repository.GetIncomeCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Rent", Type = CategoryType.Expense });

        var response = await Handler.Handle(
            new CreateIncomeCommand("Salary", 45_000m, categoryId, 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creates_The_Income_And_Returns_Its_Id()
    {
        var categoryId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        _repository.GetIncomeCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Salary", Type = CategoryType.Income });
        _repository.Add(Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>()).Returns(newId);

        var response = await Handler.Handle(
            new CreateIncomeCommand("Monthly Salary", 45_000m, categoryId, 25), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(newId, response.Result);
        await _repository.Received(1).Add(
            Arg.Is<IncomeEntity>(i =>
                i.Name == "Monthly Salary"
                && i.Amount == 45_000m
                && i.CategoryId == categoryId
                && i.RecurrenceDay == 25),
            Arg.Any<CancellationToken>());
    }
}
