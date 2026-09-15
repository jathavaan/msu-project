using FinanceOne.Api.Features.Income.UpdateIncome;

namespace FinanceOne.UnitTests.Features.Income.UpdateIncome;

public class UpdateIncomeHandlerTests
{
    private readonly IUpdateIncomeRepository _repository = Substitute.For<IUpdateIncomeRepository>();

    private UpdateIncomeHandler Handler => new(_repository);

    private static IncomeEntity AnIncome(Guid id, Guid categoryId) => new()
    {
        Id = id,
        Name = "Salary",
        Amount = 45_000m,
        CategoryId = categoryId,
        RecurrenceDay = 25,
    };

    [Fact]
    public async Task Returns_404_When_The_Income_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IncomeEntity?)null);

        var response = await Handler.Handle(
            new UpdateIncomeCommand(Guid.NewGuid(), "Salary", 45_000m, Guid.NewGuid(), 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_When_The_Target_Category_Does_Not_Exist()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(AnIncome(id, Guid.NewGuid()));
        _repository.GetIncomeCategory(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var response = await Handler.Handle(
            new UpdateIncomeCommand(id, "Salary", 45_000m, Guid.NewGuid(), 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_When_The_Target_Category_Is_An_Expense_Category()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(AnIncome(id, Guid.NewGuid()));
        _repository.GetIncomeCategory(categoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = categoryId, Name = "Rent", Type = CategoryType.Expense });

        var response = await Handler.Handle(
            new UpdateIncomeCommand(id, "Salary", 45_000m, categoryId, 25), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Applies_Every_Field_Then_Saves()
    {
        var id = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var income = AnIncome(id, Guid.NewGuid());
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(income);
        _repository.GetIncomeCategory(newCategoryId, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = newCategoryId, Name = "Freelance", Type = CategoryType.Income });

        var response = await Handler.Handle(
            new UpdateIncomeCommand(id, "Consulting Fee", 18_000m, newCategoryId, 10), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("Consulting Fee", income.Name);
        Assert.Equal(18_000m, income.Amount);
        Assert.Equal(newCategoryId, income.CategoryId);
        Assert.Equal(10, income.RecurrenceDay);
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }
}
