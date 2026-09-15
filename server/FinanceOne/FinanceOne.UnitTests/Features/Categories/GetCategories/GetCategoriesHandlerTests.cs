using FinanceOne.Api.Features.Categories.GetCategories;

namespace FinanceOne.UnitTests.Features.Categories.GetCategories;

public class GetCategoriesHandlerTests
{
    private readonly IGetCategoriesRepository _repository = Substitute.For<IGetCategoriesRepository>();

    private GetCategoriesHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_What_The_Repository_Produced()
    {
        var categories = new List<CategoryVm>
        {
            new(Guid.NewGuid(), "Rent", CategoryType.Expense),
            new(Guid.NewGuid(), "Salary", CategoryType.Income),
        };
        _repository.GetCategories(null, Arg.Any<CancellationToken>()).Returns(categories);

        var response = await Handler.Handle(new GetCategoriesQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(categories, response.Result);
    }

    // The ?type= filter is optional and passed straight through. Null has to stay null rather than
    // collapsing to Income, which is 0 both on the wire and as the enum default.
    [Theory]
    [InlineData(null)]
    [InlineData(CategoryType.Income)]
    [InlineData(CategoryType.Expense)]
    public async Task Passes_The_Type_Filter_Through_Unchanged(CategoryType? type)
    {
        _repository.GetCategories(type, Arg.Any<CancellationToken>()).Returns([]);

        await Handler.Handle(new GetCategoriesQuery(type), CancellationToken.None);

        await _repository.Received(1).GetCategories(type, Arg.Any<CancellationToken>());
    }
}
