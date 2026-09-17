using FinanceOne.Api.Features.Settings.GetSettings;

namespace FinanceOne.UnitTests.Features.Settings.GetSettings;

public class GetSettingsHandlerTests
{
    private readonly IGetSettingsRepository _repository = Substitute.For<IGetSettingsRepository>();

    private GetSettingsHandler Handler => new(_repository);

    [Fact]
    public async Task Defaults_To_Day_One_When_No_Settings_Row_Exists()
    {
        _repository.GetPeriodStartDay(Arg.Any<CancellationToken>()).Returns((int?)null);

        var response = await Handler.Handle(new GetSettingsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(1, response.Result!.PeriodStartDay);
    }

    [Fact]
    public async Task Returns_The_Configured_Period_Start_Day()
    {
        _repository.GetPeriodStartDay(Arg.Any<CancellationToken>()).Returns(25);

        var response = await Handler.Handle(new GetSettingsQuery(), CancellationToken.None);

        Assert.Equal(25, response.Result!.PeriodStartDay);
    }
}
