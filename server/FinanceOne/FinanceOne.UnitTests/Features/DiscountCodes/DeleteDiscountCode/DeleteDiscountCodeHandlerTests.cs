using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.DiscountCodes.DeleteDiscountCode;

namespace FinanceOne.UnitTests.Features.DiscountCodes.DeleteDiscountCode;

public class DeleteDiscountCodeHandlerTests
{
    private readonly IDeleteDiscountCodeRepository _repository = Substitute.For<IDeleteDiscountCodeRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();

    private DeleteDiscountCodeHandler Handler => new(_repository, _blobStorage);

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DiscountCode?)null);

        var response = await Handler.Handle(new DeleteDiscountCodeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Delete(Arg.Any<DiscountCode>(), Arg.Any<CancellationToken>());
        await _blobStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deletes_An_Existing_Discount_Code_And_Its_Image()
    {
        var id = Guid.NewGuid();
        var discountCode = new DiscountCode
        {
            Id = id,
            StoreName = "Rema 1000",
            ExpiryDate = new DateOnly(2026, 12, 31),
        };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(discountCode);

        var response = await Handler.Handle(new DeleteDiscountCodeCommand(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Delete(discountCode, Arg.Any<CancellationToken>());
        await _blobStorage.Received(1).DeleteAsync(BlobContainers.Coupons, id.ToString(), Arg.Any<CancellationToken>());
    }
}
