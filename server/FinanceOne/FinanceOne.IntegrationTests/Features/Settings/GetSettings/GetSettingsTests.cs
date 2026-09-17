using FinanceOne.Api.Features.Settings.GetSettings;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.Settings.GetSettings;

public class GetSettingsTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private GetSettingsHandler Handler => new(new GetSettingsRepository(Context));

    [Fact]
    public async Task Defaults_To_Day_One_When_No_Settings_Row_Exists()
    {
        var response = await Handler.Handle(new GetSettingsQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(1, response.Result!.PeriodStartDay);
    }

    [Fact]
    public async Task Returns_The_Configured_Period_Start_Day()
    {
        await GivenAppSettings(25);

        var response = await Handler.Handle(new GetSettingsQuery(), CancellationToken.None);

        Assert.Equal(25, response.Result!.PeriodStartDay);
    }
}
