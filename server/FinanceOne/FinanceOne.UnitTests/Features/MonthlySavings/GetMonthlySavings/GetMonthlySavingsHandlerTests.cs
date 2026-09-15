using FinanceOne.Api.Features.MonthlySavings.GetMonthlySavings;

namespace FinanceOne.UnitTests.Features.MonthlySavings.GetMonthlySavings;

public class GetMonthlySavingsHandlerTests
{
    private readonly IGetMonthlySavingsRepository _repository = Substitute.For<IGetMonthlySavingsRepository>();

    private GetMonthlySavingsHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_What_The_Repository_Produced()
    {
        var monthlySavings = new List<MonthlySavingVm>
        {
            new(Guid.NewGuid(), "Car Fund", 5_000m, Guid.NewGuid(), "New Car", 25),
            new(Guid.NewGuid(), "Holiday Fund", 1_500m, Guid.NewGuid(), "Holiday", 5),
        };
        _repository.GetMonthlySavings(Arg.Any<CancellationToken>()).Returns(monthlySavings);

        var response = await Handler.Handle(new GetMonthlySavingsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(monthlySavings, response.Result);
    }

    [Fact]
    public async Task Returns_Success_With_An_Empty_List_When_There_Are_None()
    {
        _repository.GetMonthlySavings(Arg.Any<CancellationToken>()).Returns([]);

        var response = await Handler.Handle(new GetMonthlySavingsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }
}
