namespace FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;

public static class DeleteSavingGoalEndpoint
{
    public static RouteGroupBuilder MapDeleteSavingGoal(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, DeleteSavingGoalHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new DeleteSavingGoalCommand(id), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
