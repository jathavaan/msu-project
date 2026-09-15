using FinanceOne.Api.Features.Categories.UpdateCategory;

namespace FinanceOne.UnitTests.Features.Categories.UpdateCategory;

public class UpdateCategoryHandlerTests
{
    private readonly IUpdateCategoryRepository _repository = Substitute.For<IUpdateCategoryRepository>();

    private UpdateCategoryHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_Category_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var response = await Handler.Handle(
            new UpdateCategoryCommand(Guid.NewGuid(), "Rent", CategoryType.Expense), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_409_When_Another_Category_Has_That_Name_And_Type()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = id, Name = "Groceries", Type = CategoryType.Expense });
        _repository.ExistsWithNameAndType(id, "Rent", CategoryType.Expense, Arg.Any<CancellationToken>()).Returns(true);

        var response = await Handler.Handle(
            new UpdateCategoryCommand(id, "Rent", CategoryType.Expense), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    // The duplicate check excludes the row being edited, so re-saving a category unchanged must
    // not trip the 409 against itself.
    [Fact]
    public async Task Does_Not_Conflict_With_Itself()
    {
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Name = "Rent", Type = CategoryType.Expense };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(category);
        _repository.ExistsWithNameAndType(id, "Rent", CategoryType.Expense, Arg.Any<CancellationToken>()).Returns(false);

        var response = await Handler.Handle(
            new UpdateCategoryCommand(id, "Rent", CategoryType.Expense), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task Applies_Name_And_Type_Then_Saves()
    {
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Name = "Groceries", Type = CategoryType.Expense };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(category);
        _repository.ExistsWithNameAndType(id, "Side Income", CategoryType.Income, Arg.Any<CancellationToken>()).Returns(false);

        var response = await Handler.Handle(
            new UpdateCategoryCommand(id, "Side Income", CategoryType.Income), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("Side Income", category.Name);
        Assert.Equal(CategoryType.Income, category.Type);
        await _repository.Received(1).Update(category, Arg.Any<CancellationToken>());
    }
}
