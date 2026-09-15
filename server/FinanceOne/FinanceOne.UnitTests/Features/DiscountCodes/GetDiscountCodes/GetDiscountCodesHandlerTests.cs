using FinanceOne.Api.Features.DiscountCodes.GetDiscountCodes;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.DiscountCodes.GetDiscountCodes;

public class GetDiscountCodesHandlerTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly IGetDiscountCodesRepository _repository = Substitute.For<IGetDiscountCodesRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private GetDiscountCodesHandler Handler => new(_repository, _timeProvider);

    [Fact]
    public async Task Returns_What_The_Repository_Produced()
    {
        var codes = new List<DiscountCode>
        {
            new() { Id = Guid.NewGuid(), StoreName = "Rema 1000", ExpiryDate = Today.AddDays(5) },
        };
        _repository.GetDiscountCodes(Arg.Any<int?>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(codes);

        var response = await Handler.Handle(new GetDiscountCodesQuery(null), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(codes, response.Result);
    }

    // The handler's only job is resolving "today" from the clock and handing it to the repository,
    // which is what makes the expiry window relative rather than hard-coded.
    [Fact]
    public async Task Resolves_Today_From_The_Clock_And_Passes_It_Down()
    {
        _repository.GetDiscountCodes(Arg.Any<int?>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await Handler.Handle(new GetDiscountCodesQuery(30), CancellationToken.None);

        await _repository.Received(1).GetDiscountCodes(30, Today, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Passes_A_Null_Window_Through_Unchanged()
    {
        _repository.GetDiscountCodes(Arg.Any<int?>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await Handler.Handle(new GetDiscountCodesQuery(null), CancellationToken.None);

        await _repository.Received(1).GetDiscountCodes(null, Today, Arg.Any<CancellationToken>());
    }
}
