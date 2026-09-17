namespace FinanceOne.Api.Features.CategorizationRules.GetCategorizationRules;

public static class GetCategorizationRulesEndpoint
{
    public static RouteGroupBuilder MapGetCategorizationRules(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetCategorizationRulesHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetCategorizationRulesQuery(), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
