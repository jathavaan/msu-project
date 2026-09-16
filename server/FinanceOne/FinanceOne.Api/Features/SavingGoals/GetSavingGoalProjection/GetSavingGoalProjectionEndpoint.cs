namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalProjection;

public static class GetSavingGoalProjectionEndpoint
{
    public static RouteGroupBuilder MapGetSavingGoalProjection(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}/projection", async (Guid id, GetSavingGoalProjectionHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetSavingGoalProjectionQuery(id), ct);
            return response.IsSuccess
                ? Results.Ok(response)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
