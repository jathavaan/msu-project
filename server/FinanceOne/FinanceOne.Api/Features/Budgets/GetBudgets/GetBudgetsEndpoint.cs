namespace FinanceOne.Api.Features.Budgets.GetBudgets;

public static class GetBudgetsEndpoint
{
    public static RouteGroupBuilder MapGetBudgets(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetBudgetsHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetBudgetsQuery(), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
