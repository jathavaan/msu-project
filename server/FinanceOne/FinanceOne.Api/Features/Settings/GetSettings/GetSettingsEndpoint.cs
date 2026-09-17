namespace FinanceOne.Api.Features.Settings.GetSettings;

public static class GetSettingsEndpoint
{
    public static RouteGroupBuilder MapGetSettings(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetSettingsHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetSettingsQuery(), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
