using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeImage;

namespace FinanceOne.UnitTests.Features.DiscountCodes.GetDiscountCodeImage;

public class GetDiscountCodeImageHandlerTests
{
    private readonly IGetDiscountCodeImageRepository _repository = Substitute.For<IGetDiscountCodeImageRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();

    private GetDiscountCodeImageHandler Handler => new(_repository, _blobStorage);

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DiscountCode?)null);

        var response = await Handler.Handle(new GetDiscountCodeImageQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _blobStorage.DidNotReceive().DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_When_No_Image_Has_Been_Uploaded()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(new DiscountCode
        {
            Id = id,
            StoreName = "Rema 1000",
            ExpiryDate = new DateOnly(2026, 12, 31),
        });
        _blobStorage.DownloadAsync(BlobContainers.Coupons, id.ToString(), Arg.Any<CancellationToken>())
            .Returns((DownloadedBlob?)null);

        var response = await Handler.Handle(new GetDiscountCodeImageQuery(id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Downloaded_Image()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(new DiscountCode
        {
            Id = id,
            StoreName = "Rema 1000",
            ExpiryDate = new DateOnly(2026, 12, 31),
        });
        var blob = new DownloadedBlob(new MemoryStream([1, 2, 3]), "image/png");
        _blobStorage.DownloadAsync(BlobContainers.Coupons, id.ToString(), Arg.Any<CancellationToken>()).Returns(blob);

        var response = await Handler.Handle(new GetDiscountCodeImageQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Same(blob, response.Result);
    }
}
