namespace FinanceOne.Api.Features.Income.GetIncomes;

public static class GetIncomesEndpoint
{
    public static RouteGroupBuilder MapGetIncomes(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetIncomesHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetIncomesQuery(), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
