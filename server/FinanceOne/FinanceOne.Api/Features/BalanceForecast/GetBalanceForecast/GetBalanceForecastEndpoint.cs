namespace FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

public static class GetBalanceForecastEndpoint
{
    public static RouteGroupBuilder MapGetBalanceForecast(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetBalanceForecastHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetBalanceForecastQuery(), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
