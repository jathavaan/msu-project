namespace FinanceOne.Api.Features.UpcomingPayments.GetUpcomingPayments;

public static class GetUpcomingPaymentsEndpoint
{
    public static RouteGroupBuilder MapGetUpcomingPayments(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (int? days, GetUpcomingPaymentsHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetUpcomingPaymentsQuery(days), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
