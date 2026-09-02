using FinanceOne.Api.Features.UpcomingPayments.GetUpcomingPayments;

namespace FinanceOne.Api.Features.UpcomingPayments;

public static class UpcomingPaymentsEndpoints
{
    public static void MapUpcomingPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/upcoming-payments").WithTags("UpcomingPayments");

        group.MapGetUpcomingPayments();
    }
}
