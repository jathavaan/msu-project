using FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.DiscountCodes.CreateDiscountCode;

public class CreateDiscountCodeTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private CreateDiscountCodeHandler Handler => new(new CreateDiscountCodeRepository(Context));

    [Fact]
    public async Task Persists_The_Discount_Code()
    {
        var expiry = new DateOnly(2026, 12, 31);

        var response = await Handler.Handle(
            new CreateDiscountCodeCommand("Rema 1000", "SAVE20", expiry),
            CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.DiscountCodes.SingleAsync(d => d.Id == response.Result);
        Assert.Equal("Rema 1000", saved.StoreName);
        Assert.Equal("SAVE20", saved.CodeText);
        Assert.Null(saved.CodeImageUrl);
    }

    // ExpiryDate is a DateOnly on a MySQL `date` column via an explicit DateTime conversion,
    // because MySqlDataReader cannot read a `date` back into DateOnly directly (see
    // DiscountCodeConfiguration). Only a real round-trip catches that conversion regressing.
    [Fact]
    public async Task Round_Trips_The_Expiry_Date()
    {
        var expiry = new DateOnly(2026, 12, 31);

        var response = await Handler.Handle(
            new CreateDiscountCodeCommand("Rema 1000", "SAVE20", expiry), CancellationToken.None);

        await using var context = NewContext();
        Assert.Equal(expiry, (await context.DiscountCodes.SingleAsync(d => d.Id == response.Result)).ExpiryDate);
    }

    // A code can be a scannable image (added later via Upload Discount Code Image) rather than
    // text, so CodeText must accept null.
    [Fact]
    public async Task Stores_A_Code_With_No_Text()
    {
        var response = await Handler.Handle(
            new CreateDiscountCodeCommand("Kiwi", null, new DateOnly(2026, 12, 31)), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.DiscountCodes.SingleAsync(d => d.Id == response.Result);
        Assert.Null(saved.CodeText);
        Assert.Null(saved.CodeImageUrl);
    }
}
