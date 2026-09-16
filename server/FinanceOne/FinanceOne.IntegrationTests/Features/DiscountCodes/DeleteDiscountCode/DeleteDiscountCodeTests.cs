using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.DiscountCodes.DeleteDiscountCode;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.DiscountCodes.DeleteDiscountCode;

public class DeleteDiscountCodeTests(MySqlFixture mySqlFixture, AzuriteFixture azuriteFixture)
    : IntegrationTest(mySqlFixture)
{
    private DeleteDiscountCodeHandler Handler =>
        new(new DeleteDiscountCodeRepository(Context), azuriteFixture.BlobStorageService);

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        var response = await Handler.Handle(new DeleteDiscountCodeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // DiscountCode stands alone — no FK points at it — so it is always deletable.
    [Fact]
    public async Task Removes_The_Discount_Code_Row()
    {
        var discountCode = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31));

        var response = await Handler.Handle(
            new DeleteDiscountCodeCommand(discountCode.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.False(await context.DiscountCodes.AnyAsync(d => d.Id == discountCode.Id));
    }

    [Fact]
    public async Task Leaves_Other_Discount_Codes_Alone()
    {
        var rema = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31));
        var kiwi = await GivenDiscountCode("Kiwi", new DateOnly(2027, 1, 31));

        await Handler.Handle(new DeleteDiscountCodeCommand(rema.Id), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(kiwi.Id, (await context.DiscountCodes.SingleAsync()).Id);
    }

    [Fact]
    public async Task Deletes_The_Discount_Codes_Uploaded_Image_Too()
    {
        var discountCode = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31));
        await azuriteFixture.BlobStorageService.UploadAsync(
            BlobContainers.Coupons, discountCode.Id.ToString(), new MemoryStream([1]), "image/png", CancellationToken.None);

        var response = await Handler.Handle(new DeleteDiscountCodeCommand(discountCode.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Null(await azuriteFixture.BlobStorageService.DownloadAsync(
            BlobContainers.Coupons, discountCode.Id.ToString(), CancellationToken.None));
    }

    [Fact]
    public async Task Deleting_A_Discount_Code_With_No_Image_Does_Not_Fail()
    {
        var discountCode = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31));

        var response = await Handler.Handle(new DeleteDiscountCodeCommand(discountCode.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
