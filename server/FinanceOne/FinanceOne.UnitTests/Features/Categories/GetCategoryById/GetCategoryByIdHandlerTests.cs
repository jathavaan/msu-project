using FinanceOne.Api.Features.Categories.GetCategoryById;

namespace FinanceOne.UnitTests.Features.Categories.GetCategoryById;

public class GetCategoryByIdHandlerTests
{
    private readonly IGetCategoryByIdRepository _repository = Substitute.For<IGetCategoryByIdRepository>();

    private GetCategoryByIdHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_Category_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CategoryVm?)null);

        var response = await Handler.Handle(new GetCategoryByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Category_When_It_Exists()
    {
        var id = Guid.NewGuid();
        var vm = new CategoryVm(id, "Rent", CategoryType.Expense);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(vm);

        var response = await Handler.Handle(new GetCategoryByIdQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(vm, response.Result);
    }
}
