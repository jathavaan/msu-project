using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeImage;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.DiscountCodes.GetDiscountCodeImage;

public class GetDiscountCodeImageTests(MySqlFixture mySqlFixture, AzuriteFixture azuriteFixture)
    : IntegrationTest(mySqlFixture)
{
    private GetDiscountCodeImageHandler Handler =>
        new(new GetDiscountCodeImageRepository(Context), azuriteFixture.BlobStorageService);

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetDiscountCodeImageQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_When_No_Image_Has_Been_Uploaded()
    {
        var discountCode = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31));

        var response = await Handler.Handle(new GetDiscountCodeImageQuery(discountCode.Id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Streams_Back_A_Previously_Uploaded_Image()
    {
        var discountCode = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31));
        var bytes = new byte[] { 9, 8, 7 };
        await azuriteFixture.BlobStorageService.UploadAsync(
            BlobContainers.Coupons, discountCode.Id.ToString(), new MemoryStream(bytes), "image/webp", CancellationToken.None);

        var response = await Handler.Handle(new GetDiscountCodeImageQuery(discountCode.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("image/webp", response.Result!.ContentType);
        await using var content = new MemoryStream();
        await response.Result.Content.CopyToAsync(content);
        Assert.Equal(bytes, content.ToArray());
    }
}
