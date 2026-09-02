namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodes;

public static class GetDiscountCodesEndpoint
{
    public static RouteGroupBuilder MapGetDiscountCodes(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (int? expiringWithinDays, GetDiscountCodesHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetDiscountCodesQuery(expiringWithinDays), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
