using FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

namespace FinanceOne.Api.Features.BalanceForecast;

public static class BalanceForecastEndpoints
{
    public static void MapBalanceForecastEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/balance-forecast").WithTags("BalanceForecast");

        group.MapGetBalanceForecast();
    }
}
