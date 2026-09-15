using FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;

namespace FinanceOne.UnitTests.Features.DiscountCodes.UpdateDiscountCode;

public class UpdateDiscountCodeHandlerTests
{
    private readonly IUpdateDiscountCodeRepository _repository = Substitute.For<IUpdateDiscountCodeRepository>();

    private UpdateDiscountCodeHandler Handler => new(_repository);

    private static DiscountCode ACode(Guid id) => new()
    {
        Id = id,
        StoreName = "Rema 1000",
        CodeText = "SAVE20",
        CodeImageUrl = "https://example.test/code.png",
        ExpiryDate = new DateOnly(2026, 12, 31),
    };

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DiscountCode?)null);

        var response = await Handler.Handle(
            new UpdateDiscountCodeCommand(Guid.NewGuid(), "Rema 1000", null, null, new DateOnly(2026, 12, 31)),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Applies_Every_Field_Then_Saves()
    {
        var id = Guid.NewGuid();
        var discountCode = ACode(id);
        var newExpiry = new DateOnly(2027, 3, 1);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(discountCode);

        var response = await Handler.Handle(
            new UpdateDiscountCodeCommand(id, "Kiwi", "KIWI10", "https://example.test/kiwi.png", newExpiry),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("Kiwi", discountCode.StoreName);
        Assert.Equal("KIWI10", discountCode.CodeText);
        Assert.Equal("https://example.test/kiwi.png", discountCode.CodeImageUrl);
        Assert.Equal(newExpiry, discountCode.ExpiryDate);
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }

    // The optional fields are assigned unconditionally, so sending null genuinely clears a code
    // rather than leaving the previous value in place.
    [Fact]
    public async Task Clears_The_Optional_Fields_When_Sent_As_Null()
    {
        var id = Guid.NewGuid();
        var discountCode = ACode(id);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(discountCode);

        await Handler.Handle(
            new UpdateDiscountCodeCommand(id, "Rema 1000", null, null, new DateOnly(2026, 12, 31)),
            CancellationToken.None);

        Assert.Null(discountCode.CodeText);
        Assert.Null(discountCode.CodeImageUrl);
    }
}
