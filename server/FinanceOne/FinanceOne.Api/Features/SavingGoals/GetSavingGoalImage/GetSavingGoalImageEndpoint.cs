namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalImage;

public static class GetSavingGoalImageEndpoint
{
    public static RouteGroupBuilder MapGetSavingGoalImage(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}/image", async (Guid id, GetSavingGoalImageHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetSavingGoalImageQuery(id), ct);
            return response.IsSuccess
                ? Results.Stream(response.Result!.Content, response.Result.ContentType)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
