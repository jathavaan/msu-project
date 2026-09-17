using FinanceOne.Api.Features.SpendTrends.GetCategorySpendTrend;

namespace FinanceOne.Api.Features.SpendTrends;

public static class SpendTrendsEndpoints
{
    public static void MapSpendTrendsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/spend-trends").WithTags("SpendTrends");

        group.MapGetCategorySpendTrend();
    }
}
