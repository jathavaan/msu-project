using FinanceOne.Api.Features.Settings.GetSettings;
using FinanceOne.Api.Features.Settings.UpdateSettings;

namespace FinanceOne.Api.Features.Settings;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings");

        group.MapGetSettings();
        group.MapUpdateSettings();
    }
}
