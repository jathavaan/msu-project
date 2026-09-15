using FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.DiscountCodes.UpdateDiscountCode;

public class UpdateDiscountCodeTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateDiscountCodeHandler Handler => new(new UpdateDiscountCodeRepository(Context));

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new UpdateDiscountCodeCommand(Guid.NewGuid(), "Rema 1000", null, null, new DateOnly(2026, 12, 31)),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Persists_Every_Changed_Field()
    {
        var discountCode = await GivenDiscountCode("Rema 1000", new DateOnly(2026, 12, 31), codeText: "SAVE20");
        var newExpiry = new DateOnly(2027, 3, 1);

        var response = await Handler.Handle(
            new UpdateDiscountCodeCommand(discountCode.Id, "Kiwi", "KIWI10", "https://example.test/kiwi.png", newExpiry),
            CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.DiscountCodes.SingleAsync(d => d.Id == discountCode.Id);
        Assert.Equal("Kiwi", saved.StoreName);
        Assert.Equal("KIWI10", saved.CodeText);
        Assert.Equal("https://example.test/kiwi.png", saved.CodeImageUrl);
        Assert.Equal(newExpiry, saved.ExpiryDate);
    }

    // The optional columns are assigned unconditionally, so sending null genuinely clears a stored
    // code rather than leaving the old value behind.
    [Fact]
    public async Task Clears_The_Optional_Fields_When_Sent_As_Null()
    {
        var discountCode = await GivenDiscountCode(
            "Rema 1000",
            new DateOnly(2026, 12, 31),
            codeText: "SAVE20",
            codeImageUrl: "https://example.test/code.png");

        await Handler.Handle(
            new UpdateDiscountCodeCommand(discountCode.Id, "Rema 1000", null, null, new DateOnly(2026, 12, 31)),
            CancellationToken.None);

        await using var context = NewContext();
        var saved = await context.DiscountCodes.SingleAsync(d => d.Id == discountCode.Id);
        Assert.Null(saved.CodeText);
        Assert.Null(saved.CodeImageUrl);
    }
}
