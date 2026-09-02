namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;

public static class GetSavingGoalByIdEndpoint
{
    public static RouteGroupBuilder MapGetSavingGoalById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, GetSavingGoalByIdHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetSavingGoalByIdQuery(id), ct);
            return response.IsSuccess
                ? Results.Ok(response.Result)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
