namespace FinanceOne.Api.Features.Settings.UpdateSettings;

public static class UpdateSettingsEndpoint
{
    public static RouteGroupBuilder MapUpdateSettings(this RouteGroupBuilder group)
    {
        group.MapPut("/", async (UpdateSettingsCommand command, UpdateSettingsHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.NoContent()
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<UpdateSettingsCommand>>();

        return group;
    }
}
