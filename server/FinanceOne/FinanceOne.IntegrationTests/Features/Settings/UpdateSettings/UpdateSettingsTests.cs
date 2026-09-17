using FinanceOne.Api.Features.Settings.UpdateSettings;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Settings.UpdateSettings;

public class UpdateSettingsTests(MySqlFixture fixture) : IntegrationTest(fixture)
{
    private UpdateSettingsHandler Handler => new(new UpdateSettingsRepository(Context));

    [Fact]
    public async Task Creates_A_Row_On_First_Write()
    {
        var response = await Handler.Handle(new UpdateSettingsCommand(25), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await using var context = NewContext();
        Assert.Equal(25, (await context.AppSettings.SingleAsync()).PeriodStartDay);
    }

    [Fact]
    public async Task Updates_The_Existing_Row_Instead_Of_Adding_A_Second_One()
    {
        await GivenAppSettings(1);

        await Handler.Handle(new UpdateSettingsCommand(25), CancellationToken.None);

        await using var context = NewContext();
        var allSettings = await context.AppSettings.ToListAsync();
        var settings = Assert.Single(allSettings);
        Assert.Equal(25, settings.PeriodStartDay);
    }
}
