using FinanceOne.Api.Features.Categories.CreateCategory;

namespace FinanceOne.UnitTests.Features.Categories.CreateCategory;

public class CreateCategoryHandlerTests
{
    private readonly ICreateCategoryRepository _repository = Substitute.For<ICreateCategoryRepository>();

    private CreateCategoryHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_409_When_Name_And_Type_Already_Exist()
    {
        _repository.ExistsWithNameAndType("Rent", CategoryType.Expense, Arg.Any<CancellationToken>()).Returns(true);

        var response = await Handler.Handle(new CreateCategoryCommand("Rent", CategoryType.Expense), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        await _repository.DidNotReceive().Add(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    // Uniqueness is per (name, type) pair, so the same name is allowed once as Income and once as
    // Expense — gifts you receive vs gifts you buy.
    [Fact]
    public async Task Allows_The_Same_Name_Under_A_Different_Type()
    {
        _repository.ExistsWithNameAndType("Gifts", CategoryType.Income, Arg.Any<CancellationToken>()).Returns(false);
        _repository.Add(Arg.Any<Category>(), Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

        var response = await Handler.Handle(new CreateCategoryCommand("Gifts", CategoryType.Income), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task Creates_Category_And_Returns_Its_Id()
    {
        var newId = Guid.NewGuid();
        _repository.ExistsWithNameAndType(Arg.Any<string>(), Arg.Any<CategoryType>(), Arg.Any<CancellationToken>()).Returns(false);
        _repository.Add(Arg.Any<Category>(), Arg.Any<CancellationToken>()).Returns(newId);

        var response = await Handler.Handle(new CreateCategoryCommand("Rent", CategoryType.Expense), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(newId, response.Result);
        await _repository.Received(1).Add(
            Arg.Is<Category>(c => c.Name == "Rent" && c.Type == CategoryType.Expense),
            Arg.Any<CancellationToken>());
    }
}
