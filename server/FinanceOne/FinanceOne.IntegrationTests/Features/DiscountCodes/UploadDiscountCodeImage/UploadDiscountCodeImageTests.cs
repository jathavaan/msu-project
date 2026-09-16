using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.DiscountCodes.UploadDiscountCodeImage;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.DiscountCodes.UploadDiscountCodeImage;

public class UploadDiscountCodeImageTests(MySqlFixture mySqlFixture, AzuriteFixture azuriteFixture)
    : IntegrationTest(mySqlFixture)
{
    private UploadDiscountCodeImageHandler Handler =>
        new(new UploadDiscountCodeImageRepository(Context), azuriteFixture.BlobStorageService);

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new UploadDiscountCodeImageCommand(Guid.NewGuid(), new MemoryStream([1]), "image/png", 1),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Uploads_The_Image_And_Persists_The_Proxied_CodeImageUrl()
    {
        var discountCode = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31));
        var bytes = new byte[] { 1, 2, 3 };

        var response = await Handler.Handle(
            new UploadDiscountCodeImageCommand(discountCode.Id, new MemoryStream(bytes), "image/png", bytes.Length),
            CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.DiscountCodes.SingleAsync(d => d.Id == discountCode.Id);
        Assert.Equal($"/api/discount-codes/{discountCode.Id}/image", saved.CodeImageUrl);

        var downloaded = await azuriteFixture.BlobStorageService.DownloadAsync(
            BlobContainers.Coupons, discountCode.Id.ToString(), CancellationToken.None);
        Assert.NotNull(downloaded);
        Assert.Equal("image/png", downloaded.ContentType);
    }

    [Fact]
    public async Task Uploading_Again_Replaces_The_Previous_Image()
    {
        var discountCode = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31));
        await Handler.Handle(
            new UploadDiscountCodeImageCommand(discountCode.Id, new MemoryStream([1]), "image/png", 1),
            CancellationToken.None);

        await Handler.Handle(
            new UploadDiscountCodeImageCommand(discountCode.Id, new MemoryStream([2, 2]), "image/jpeg", 2),
            CancellationToken.None);

        var downloaded = await azuriteFixture.BlobStorageService.DownloadAsync(
            BlobContainers.Coupons, discountCode.Id.ToString(), CancellationToken.None);
        Assert.Equal("image/jpeg", downloaded!.ContentType);
    }
}
