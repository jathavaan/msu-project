using FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeById;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.DiscountCodes.GetDiscountCodeById;

public class GetDiscountCodeByIdTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetDiscountCodeByIdHandler Handler => new(new GetDiscountCodeByIdRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetDiscountCodeByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Discount_Code_When_It_Exists()
    {
        var expiry = new DateOnly(2026, 12, 31);
        var discountCode = await GivenDiscountCode("Rema 1000", expiry, codeText: "SAVE20");

        var response = await Handler.Handle(
            new GetDiscountCodeByIdQuery(discountCode.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("Rema 1000", response.Result!.StoreName);
        Assert.Equal("SAVE20", response.Result.CodeText);
        Assert.Equal(expiry, response.Result.ExpiryDate);
    }
}
