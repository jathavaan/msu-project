using FinanceOne.Api.Features.Categories.GetCategoryById;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Categories.GetCategoryById;

public class GetCategoryByIdTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetCategoryByIdHandler Handler => new(new GetCategoryByIdRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Category_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetCategoryByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Category_When_It_Exists()
    {
        var category = await GivenCategory("Salary", CategoryType.Income);

        var response = await Handler.Handle(new GetCategoryByIdQuery(category.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(category.Id, response.Result!.Id);
        Assert.Equal("Salary", response.Result.Name);
        Assert.Equal(CategoryType.Income, response.Result.Type);
    }
}
