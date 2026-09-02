namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoals;

public static class GetSavingGoalsEndpoint
{
    public static RouteGroupBuilder MapGetSavingGoals(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetSavingGoalsHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetSavingGoalsQuery(), ct);
            return Results.Ok(response.Result);
        });

        return group;
    }
}
