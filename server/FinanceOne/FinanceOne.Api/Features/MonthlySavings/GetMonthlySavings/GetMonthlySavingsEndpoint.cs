namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavings;

public static class GetMonthlySavingsEndpoint
{
    public static RouteGroupBuilder MapGetMonthlySavings(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetMonthlySavingsHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetMonthlySavingsQuery(), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
