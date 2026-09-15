using FinanceOne.Api.Features.DiscountCodes.GetDiscountCodes;
using FinanceOne.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.IntegrationTests.Features.DiscountCodes.GetDiscountCodes;

public class GetDiscountCodesTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetDiscountCodesHandler Handler => new(new GetDiscountCodesRepository(Context), _timeProvider);

    [Fact]
    public async Task Returns_An_Empty_List_When_There_Are_None()
    {
        var response = await Handler.Handle(new GetDiscountCodesQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }

    // With no window the query is unfiltered, so already-expired codes are still listed — the
    // frontend decides how to present them.
    [Fact]
    public async Task Returns_Every_Code_Including_Expired_Ones_When_No_Window_Is_Given()
    {
        await GivenDiscountCode("Expired", Today.AddDays(-10));
        await GivenDiscountCode("Current", Today.AddDays(10));

        var response = await Handler.Handle(new GetDiscountCodesQuery(null), CancellationToken.None);

        Assert.Equal(2, response.Result!.Count);
    }

    // The window is a closed range [today, today + n], so it drops anything already expired as well
    // as anything expiring beyond the cutoff.
    [Fact]
    public async Task Filters_To_Codes_Expiring_Within_The_Window()
    {
        await GivenDiscountCode("Expired", Today.AddDays(-1));
        await GivenDiscountCode("Expires today", Today);
        await GivenDiscountCode("Within window", Today.AddDays(15));
        await GivenDiscountCode("On the cutoff", Today.AddDays(30));
        await GivenDiscountCode("Beyond window", Today.AddDays(31));

        var response = await Handler.Handle(new GetDiscountCodesQuery(30), CancellationToken.None);

        Assert.Equal(
            ["Expires today", "Within window", "On the cutoff"],
            response.Result!.Select(d => d.StoreName));
    }

    [Fact]
    public async Task Orders_By_Expiry_Date()
    {
        await GivenDiscountCode("Latest", Today.AddDays(30));
        await GivenDiscountCode("Soonest", Today.AddDays(2));
        await GivenDiscountCode("Middle", Today.AddDays(10));

        var response = await Handler.Handle(new GetDiscountCodesQuery(null), CancellationToken.None);

        Assert.Equal(["Soonest", "Middle", "Latest"], response.Result!.Select(d => d.StoreName));
    }

    [Fact]
    public async Task Round_Trips_The_Expiry_Date_Through_The_Query()
    {
        await GivenDiscountCode("Rema 1000", Today.AddDays(10));

        var response = await Handler.Handle(new GetDiscountCodesQuery(null), CancellationToken.None);

        Assert.Equal(Today.AddDays(10), Assert.Single(response.Result!).ExpiryDate);
    }
}
