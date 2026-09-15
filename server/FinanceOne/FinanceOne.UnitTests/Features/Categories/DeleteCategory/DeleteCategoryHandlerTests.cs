using FinanceOne.Api.Features.Categories.DeleteCategory;

namespace FinanceOne.UnitTests.Features.Categories.DeleteCategory;

public class DeleteCategoryHandlerTests
{
    private readonly IDeleteCategoryRepository _repository = Substitute.For<IDeleteCategoryRepository>();

    private DeleteCategoryHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_Category_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var response = await Handler.Handle(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // Every FK onto Category is OnDelete(Restrict), so deleting a referenced category would throw
    // at the database. The handler turns that into an expected 409 instead.
    [Fact]
    public async Task Returns_409_When_Category_Is_Still_Referenced()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>())
            .Returns(new Category { Id = id, Name = "Rent", Type = CategoryType.Expense });
        _repository.IsReferenced(id, Arg.Any<CancellationToken>()).Returns(true);

        var response = await Handler.Handle(new DeleteCategoryCommand(id), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        await _repository.DidNotReceive().Delete(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deletes_An_Unreferenced_Category()
    {
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Name = "Rent", Type = CategoryType.Expense };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(category);
        _repository.IsReferenced(id, Arg.Any<CancellationToken>()).Returns(false);

        var response = await Handler.Handle(new DeleteCategoryCommand(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Delete(category, Arg.Any<CancellationToken>());
    }
}
