using FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;

namespace FinanceOne.UnitTests.Features.DiscountCodes.CreateDiscountCode;

public class CreateDiscountCodeHandlerTests
{
    private readonly ICreateDiscountCodeRepository _repository = Substitute.For<ICreateDiscountCodeRepository>();

    private CreateDiscountCodeHandler Handler => new(_repository);

    // DiscountCode references nothing, so this handler has no lookup and no failure branch — it
    // maps the command onto the entity and saves.
    [Fact]
    public async Task Creates_The_Discount_Code_And_Returns_Its_Id()
    {
        var newId = Guid.NewGuid();
        var expiry = new DateOnly(2026, 12, 31);
        _repository.Add(Arg.Any<DiscountCode>(), Arg.Any<CancellationToken>()).Returns(newId);

        var response = await Handler.Handle(
            new CreateDiscountCodeCommand("Rema 1000", "SAVE20", "https://example.test/code.png", expiry),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(newId, response.Result);
        await _repository.Received(1).Add(
            Arg.Is<DiscountCode>(d =>
                d.StoreName == "Rema 1000"
                && d.CodeText == "SAVE20"
                && d.CodeImageUrl == "https://example.test/code.png"
                && d.ExpiryDate == expiry),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Carries_Null_Code_Fields_Through_Unchanged()
    {
        _repository.Add(Arg.Any<DiscountCode>(), Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

        await Handler.Handle(
            new CreateDiscountCodeCommand("Rema 1000", null, null, new DateOnly(2026, 12, 31)),
            CancellationToken.None);

        await _repository.Received(1).Add(
            Arg.Is<DiscountCode>(d => d.CodeText == null && d.CodeImageUrl == null),
            Arg.Any<CancellationToken>());
    }
}
