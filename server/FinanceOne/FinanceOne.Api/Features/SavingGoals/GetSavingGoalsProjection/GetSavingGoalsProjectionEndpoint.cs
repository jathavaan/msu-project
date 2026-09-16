namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalsProjection;

public static class GetSavingGoalsProjectionEndpoint
{
    public static RouteGroupBuilder MapGetSavingGoalsProjection(this RouteGroupBuilder group)
    {
        group.MapGet("/projection", async (int? years, GetSavingGoalsProjectionHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetSavingGoalsProjectionQuery(years), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
