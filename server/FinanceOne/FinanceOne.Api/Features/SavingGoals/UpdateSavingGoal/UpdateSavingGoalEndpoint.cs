namespace FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;

public static class UpdateSavingGoalEndpoint
{
    public static RouteGroupBuilder MapUpdateSavingGoal(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateSavingGoalCommand command, UpdateSavingGoalHandler handler, CancellationToken ct) =>
            {
                if (id != command.Id)
                {
                    return Results.BadRequest();
                }

                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.NoContent()
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<UpdateSavingGoalCommand>>();

        return group;
    }
}
